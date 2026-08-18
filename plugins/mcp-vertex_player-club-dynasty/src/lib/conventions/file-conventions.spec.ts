import { describe, expect, it } from 'vitest';

import { FILE_CONVENTIONS, SOLID_PRINCIPLES, WORKSPACE_RULES } from './file-conventions.constant';
import { validatePath } from './file-conventions.helper';
import { renderTemplate, templateFor } from './file-assistant.helper';

describe('architecture-conventions', () => {
	it('recognises every supported file extension', () => {
		for (const [kind, rule] of Object.entries(FILE_CONVENTIONS)) {
			const result = validatePath(`tools/helpers/example${rule.extension}`);
			expect(result.kind).toBe(kind);
			expect(result.ok).toBe(true);
		}
	});

	it('rejects unknown extensions', () => {
		const result = validatePath('tools/foo.ts');
		expect(result.kind).toBe('unknown');
		expect(result.ok).toBe(false);
	});

	it('surfaces the SOLID principles', () => {
		expect(Object.keys(SOLID_PRINCIPLES)).toEqual(['srp', 'ocp', 'lsp', 'isp', 'dip']);
	});

	it('publishes the workspace rules', () => {
		expect(WORKSPACE_RULES.length).toBeGreaterThan(0);
	});

	it('renders a template for every kind', () => {
		for (const kind of Object.keys(FILE_CONVENTIONS) as Array<keyof typeof FILE_CONVENTIONS>) {
			const template = templateFor(kind);
			expect(template.extension).toBe(FILE_CONVENTIONS[kind].extension);
			const body = renderTemplate(kind);
			expect(body.startsWith(template.header.split('\n')[0])).toBe(true);
		}
	});
});
