# tools/scripts/reference/vbox-mouse.ps1
#
# Thin PowerShell executor for VirtualBox's COM `IMouse` interface. This is
# the only way to inject mouse input into a guest without touching the
# host's real cursor or window focus: `IMouse.PutMouseEvent` writes straight
# into the VM's virtual PS/2 controller, bypassing Windows' own input stack
# entirely (see `docs/wiki/in-game/como-esta-montado.md`).
#
# Invoked by `tools/helpers/reference/vbox-mouse.helper.ts` — the decision
# logic (calibration, step size, when to click) lives in TS; this script
# only executes one already-decided move/click against the COM API, because
# that API has no Bun/Node binding and PowerShell is the only host-side way
# to reach it.
#
# Relative-only: this guest (Windows 98) never negotiates USB HID absolute
# positioning, so `PutMouseEventAbsolute` is a no-op here. Every move is a
# sequence of small relative deltas (steps of 3, ~8ms apart) to avoid
# Windows' pointer-ballistics distorting one large jump.
#
# IMPORTANT — do not "reset" with one huge single-call delta (e.g.
# PutMouseEvent(-5000,-5000,...)). Real PS/2 packets encode dx/dy as a
# small signed field per packet; a single huge value gets silently
# truncated/wrapped rather than clamped, so it lands at an unpredictable
# offset instead of the true (0,0) corner — this produced hours of
# irreproducible coordinate drift before being diagnosed (confirmed via a
# button-held drag: the resulting selection-rectangle's corner did not
# start at screen (0,0)). The fix is to walk to the corner in the same
# small, packet-safe steps as any other move.

param(
    [Parameter(Mandatory = $true)][string]$VmName,
    [switch]$ResetCorner,
    [int]$Dx = 0,
    [int]$Dy = 0,
    [switch]$Click,
    [switch]$RightClick,
    [int]$ClickDelayMs = 150,
    [int]$HoldMs = 120,
    [Nullable[int]]$DragToDx = $null,
    [Nullable[int]]$DragToDy = $null
)

$ErrorActionPreference = 'Stop'

$vbox = New-Object -ComObject VirtualBox.VirtualBox
$machine = $vbox.FindMachine($VmName)
$session = New-Object -ComObject VirtualBox.Session
$machine.LockMachine($session, 1) # 1 = LockType_Shared; must be an int, a string throws
$console = $session.Console
$mouse = $console.Mouse

function Move-Walked([int]$totalX, [int]$totalY, [int]$stepSize, [int]$buttonState) {
    $remainingX = $totalX
    $remainingY = $totalY
    while ($remainingX -ne 0 -or $remainingY -ne 0) {
        $stepX = [Math]::Sign($remainingX) * [Math]::Min([Math]::Abs($remainingX), $stepSize)
        $stepY = [Math]::Sign($remainingY) * [Math]::Min([Math]::Abs($remainingY), $stepSize)
        $mouse.PutMouseEvent($stepX, $stepY, 0, 0, $buttonState)
        $remainingX -= $stepX
        $remainingY -= $stepY
        Start-Sleep -Milliseconds 8
    }
}

try {
    if ($ResetCorner) {
        # Walk to the corner in packet-safe steps (see the note above) —
        # -3000 is far beyond any real resolution, and a step of 20 stays
        # safely under any plausible per-packet magnitude limit.
        Move-Walked -totalX -3000 -totalY -3000 -stepSize 20 -buttonState 0
        Start-Sleep -Milliseconds 100
    }

    Move-Walked -totalX $Dx -totalY $Dy -stepSize 3 -buttonState 0

    if ($Click) {
        Start-Sleep -Milliseconds $ClickDelayMs
        $mouse.PutMouseEvent(0, 0, 0, 0, 1)
        Start-Sleep -Milliseconds $HoldMs
        $mouse.PutMouseEvent(0, 0, 0, 0, 0)
    }

    if ($RightClick) {
        Start-Sleep -Milliseconds $ClickDelayMs
        $mouse.PutMouseEvent(0, 0, 0, 0, 2)
        Start-Sleep -Milliseconds $HoldMs
        $mouse.PutMouseEvent(0, 0, 0, 0, 0)
    }

    if ($null -ne $DragToDx -and $null -ne $DragToDy) {
        # Press at the current (post-move) position, walk to the target
        # with the button held (buttonState=1 on every packet along the
        # way — this is what makes it a drag rather than a teleport), then
        # release. Same packet-safe step size as any other move.
        Start-Sleep -Milliseconds $ClickDelayMs
        $mouse.PutMouseEvent(0, 0, 0, 0, 1)
        Start-Sleep -Milliseconds 80
        Move-Walked -totalX $DragToDx -totalY $DragToDy -stepSize 3 -buttonState 1
        Start-Sleep -Milliseconds $HoldMs
        $mouse.PutMouseEvent(0, 0, 0, 0, 0)
    }
}
finally {
    $session.UnlockMachine()
}

Write-Output "ok reset=$ResetCorner dx=$Dx dy=$Dy click=$Click rightClick=$RightClick holdMs=$HoldMs"
