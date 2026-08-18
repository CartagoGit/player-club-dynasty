import { describe, expect, it } from 'vitest';

import plugin from '../../src/index';

interface IFakeMcpServer {
	registerTool: (id: string) => void;
}

describe('@player-club-dynasty/mcp-vertex-plugin', () => {
	it('exports the canonical plugin name + version', () => {
		expect(plugin.name).toBe('player-club-dynasty');
		expect(plugin.version).toMatch(/^\d+\.\d+\.\d+/);
	});

	it('registers the eight tools (3 domain + 5 audio)', async () => {
		const ids: string[] = [];
		const server: IFakeMcpServer = {
			registerTool: (id) => {
				ids.push(id);
			},
		};
		const ctx = {
			options: {},
			namespacePrefix: 'player-club-dynasty',
			workspace: { root: process.cwd(), resolve: (p: string) => `${process.cwd()}/${p}` },
		} as unknown as Parameters<NonNullable<typeof plugin.register>>[0];
		const result = await plugin.register(ctx);
		if (!result || !result.tools) throw new Error('plugin did not return tools');
		for (const tool of result.tools) {
			await tool.register(server as unknown as Parameters<typeof tool.register>[0]);
		}
		expect(ids).toContain('player-club-dynasty_list_clubs');
		expect(ids).toContain('player-club-dynasty_get_squad');
		expect(ids).toContain('player-club-dynasty_current_season');
		expect(ids).toContain('player-club-dynasty_audio_stage');
		expect(ids).toContain('player-club-dynasty_audio_list_variants');
		expect(ids).toContain('player-club-dynasty_audio_listen');
		expect(ids).toContain('player-club-dynasty_audio_approve');
		expect(ids).toContain('player-club-dynasty_audio_deprecate');
	});

	it('describes itself in plain language', () => {
		expect(plugin.describe).toMatch(/player[- ]?club/i);
	});

	it('parses the options schema with sane defaults', () => {
		const schema = plugin.optionsSchema;
		if (!schema) throw new Error('expected optionsSchema');
		const parsed = schema.safeParse({});
		expect(parsed.success).toBe(true);
	});
});