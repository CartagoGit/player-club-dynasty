import { describe, expect, it } from 'vitest';

import { findMachinePathLines } from './portable-paths.script.ts';

describe('findMachinePathLines', () => {
	it('flags a delendai line that names a machine path', () => {
		const files = new Map([
			['a.json', '{\n"$schema": "/home/dev/.local/share/delendai/stable/x.json"\n}'],
			['b.json', '"/Users/dev/delendai/x"'],
		]);

		expect(findMachinePathLines(files)).toEqual(['a.json:2', 'b.json:1']);
	});

	it('accepts files that only name the runtime link', () => {
		const files = new Map([['a.json', '"$schema": "./.cache/delendai-runtime/x.json"']]);

		expect(findMachinePathLines(files)).toEqual([]);
	});

	it('ignores machine paths unrelated to delendai and proposal history', () => {
		const files = new Map([
			['t.cs', '"/home/user/game/locale"'],
			['docs/delendai/proposals/done/x.md', '/home/dev/delendai'],
		]);

		expect(findMachinePathLines(files)).toEqual([]);
	});
});
