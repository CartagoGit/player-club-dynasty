import { defineConfig } from 'vitest/config';

export default defineConfig({
	test: {
		globals: true,
		environment: 'node',
		include: [
			'scripts/**/*.spec.ts',
			'tools/scripts/**/*.spec.ts',
			'tools/helpers/**/*.spec.ts',
		],
	},
});
