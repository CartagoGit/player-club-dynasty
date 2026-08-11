import { describe, expect, it } from 'vitest';

import plugin from '../../src/index';

describe('@player-club-dynasty/mcp-vertex-plugin', () => {
	it('exports the canonical plugin name + version', () => {
		expect(plugin.name).toBe('player-club-dynasty');
		expect(plugin.version).toMatch(/^\d+\.\d+\.\d+/);
	});

	it('declares the three domain tools', () => {
		expect(plugin.register).toBeDefined();
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
