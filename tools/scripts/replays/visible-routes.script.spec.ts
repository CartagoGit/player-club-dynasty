import { describe, expect, it } from 'vitest';

import {
	checkProposal,
	declaresVisibleCapability,
	parseDeclaredFiles,
	parseProposal,
} from './visible-routes.script.ts';

const proposal = (options: {
	id: string;
	kind: string;
	status: string;
	track: string;
	files: readonly string[];
	body: string;
}): string =>
	[
		'---',
		`id: ${options.id}`,
		`title: "A proposal used by the gate specification"`,
		`kind: ${options.kind}`,
		`status: ${options.status}`,
		'type: proposal',
		`track: ${options.track}`,
		'date: 2026-08-18',
		'---',
		'',
		'## Slices',
		'',
		'### S1 — The slice',
		`- **Files**: ${options.files.map((file) => `\`${file}\``).join(', ')}`,
		'- **Gate**: e2e',
		'- acceptance:',
		`  - "${options.body}"`,
		'',
	].join('\n');

const nothingExists = (): boolean => false;
const everythingExists = (): boolean => true;

describe('parseDeclaredFiles', () => {
	it('collects every backticked path from the slice Files bullets', () => {
		const markdown = proposal({
			id: 'f00099',
			kind: 'feat',
			status: 'ready',
			track: 'product',
			files: ['game/godot/Scripts/Shell/MenuController.cs', 'game/src/PCDynasty.Core/Thing.cs'],
			body: 'The menu shows the thing.',
		});

		expect(parseDeclaredFiles(markdown)).toEqual([
			'game/godot/Scripts/Shell/MenuController.cs',
			'game/src/PCDynasty.Core/Thing.cs',
		]);
	});
});

describe('declaresVisibleCapability', () => {
	it('is false for a plan, which delegates routes to its children', () => {
		const record = parseProposal(
			'q00001.md',
			proposal({
				id: 'q00001',
				kind: 'plan',
				status: 'ready',
				track: 'roadmap',
				files: [],
				body: 'Every child ships a Godot screen.',
			}),
		);

		expect(declaresVisibleCapability(record)).toBe(false);
	});

	it('is false for an audit, which produces a report rather than a capability', () => {
		const record = parseProposal(
			'a00099.md',
			proposal({
				id: 'a00099',
				kind: 'audit',
				status: 'review',
				track: 'architecture',
				files: ['docs/architecture/0011-report.md'],
				body: 'The report inspects the Godot workspace and SmokeBoot.',
			}),
		);

		expect(declaresVisibleCapability(record)).toBe(false);
	});

	it('is false for a behaviour proposal that never mentions anything visible', () => {
		const record = parseProposal(
			'f00098.md',
			proposal({
				id: 'f00098',
				kind: 'feat',
				status: 'ready',
				track: 'product',
				files: ['game/src/PCDynasty.Core/Ratings.cs'],
				body: 'Ratings are computed with fixed point arithmetic.',
			}),
		);

		expect(declaresVisibleCapability(record)).toBe(false);
	});

	it('is true for an open behaviour proposal whatever track it chose', () => {
		const record = parseProposal(
			'f00097.md',
			proposal({
				id: 'f00097',
				kind: 'feat',
				status: 'ready',
				track: 'invented-track',
				files: ['game/src/PCDynasty.Knowledge/Scouting/Assignments.cs'],
				body: 'The scouting screen lists the candidates.',
			}),
		);

		expect(declaresVisibleCapability(record)).toBe(true);
	});
});

describe('checkProposal', () => {
	it('fails a proposal that claims a visible capability with no presentation file', () => {
		const record = parseProposal(
			'f00096.md',
			proposal({
				id: 'f00096',
				kind: 'feat',
				status: 'ready',
				track: 'product',
				files: ['game/src/PCDynasty.Knowledge/Scouting/Assignments.cs'],
				body: 'The scouting screen lists the candidates in Godot.',
			}),
		);

		const violation = checkProposal(record, nothingExists);

		expect(violation).not.toBeNull();
		expect(violation?.id).toBe('f00096');
		expect(violation?.reason).toContain('game/godot/');
	});

	it('passes the same proposal once it names a presentation file', () => {
		const record = parseProposal(
			'f00096.md',
			proposal({
				id: 'f00096',
				kind: 'feat',
				status: 'ready',
				track: 'product',
				files: [
					'game/src/PCDynasty.Knowledge/Scouting/Assignments.cs',
					'game/godot/Scripts/Workspace/Screens/ScoutingScreen.cs',
				],
				body: 'The scouting screen lists the candidates in Godot.',
			}),
		);

		expect(checkProposal(record, nothingExists)).toBeNull();
	});

	it('fails a closed proposal whose declared presentation files are absent from the build', () => {
		const record = parseProposal(
			'f00095.md',
			proposal({
				id: 'f00095',
				kind: 'feat',
				status: 'done',
				track: 'product',
				files: ['game/godot/Scripts/Workspace/Screens/GhostScreen.cs'],
				body: 'The ghost screen is navigable from the sidebar.',
			}),
		);

		const violation = checkProposal(record, nothingExists);

		expect(violation?.reason).toContain('exist on disk');
	});

	it('passes a closed proposal whose presentation files are present', () => {
		const record = parseProposal(
			'f00095.md',
			proposal({
				id: 'f00095',
				kind: 'feat',
				status: 'done',
				track: 'product',
				files: ['game/godot/Scripts/Workspace/Screens/MarketScreen.cs'],
				body: 'The market screen is navigable from the sidebar.',
			}),
		);

		expect(checkProposal(record, everythingExists)).toBeNull();
	});
});
