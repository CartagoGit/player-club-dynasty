/**
 * plugins/mcp-vertex_player-club-dynasty/src/lib/conventions/file-assistant.helper.ts
 *
 * Builder for the canonical header + imports of every supported file
 * kind. The plugin exposes it through a prompt so agents can ask
 * "give me the template for a new helper in tools/helpers" before
 * typing the file by hand.
 */

import { type FileKind, FILE_CONVENTIONS } from './file-conventions.constants';

export const TEMPLATE_LICENSE_HEADER = `/**`;

export interface IFileTemplate {
	readonly kind: FileKind;
	readonly extension: string;
	readonly description: string;
	readonly header: string;
	readonly imports: readonly string[];
	readonly body: string;
}

const fileTemplate = (kind: FileKind, body: string, imports: readonly string[] = []): IFileTemplate => ({
	kind,
	extension: FILE_CONVENTIONS[kind].extension,
	description: FILE_CONVENTIONS[kind].role,
	header: `#!/usr/bin/env bun\n/**\n * ${FILE_CONVENTIONS[kind].role}\n */`,
	imports,
	body,
});

export const templateFor = (kind: FileKind): IFileTemplate => {
	switch (kind) {
		case 'script':
			return fileTemplate(
				'script',
				`\nconst main = async (): Promise<number> => {\n  // TODO: implement.\n  return 0;\n};\n\nprocess.exit(await main());\n`,
			);
		case 'helper':
			return fileTemplate(
				'helper',
				`\n/** One function, one responsibility. */\nexport const helperName = (...): void => {\n  // TODO: implement.\n};\n`,
			);
		case 'interface':
			return fileTemplate(
				'interface',
				`\n/** Shape consumed by the helper layer. */\nexport interface IExample {\n  readonly id: string;\n}\n`,
			);
		case 'constants':
			return fileTemplate(
				'constants',
				`\n/** No logic; frozen values for the rest of the tool layer. */\nexport const EXAMPLE_CONSTANT = 'value' as const;\n`,
			);
		case 'spec':
			return fileTemplate(
				'spec',
				`\nimport { describe, expect, it } from 'vitest';\n\ndescribe('helperName', () => {\n  it('does the thing', () => {\n    expect(true).toBe(true);\n  });\n});\n`,
				['import { describe, expect, it } from "vitest";'],
			);
	}
};

/** Render a template as a string suitable for `create_file`. */
export const renderTemplate = (kind: FileKind): string => {
	const template = templateFor(kind);
	return `${template.header}\n${template.imports.join('\n')}${template.body}`;
};
