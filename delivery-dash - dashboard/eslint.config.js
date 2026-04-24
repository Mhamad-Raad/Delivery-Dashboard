import js from '@eslint/js'
import globals from 'globals'
import reactHooks from 'eslint-plugin-react-hooks'
import reactRefresh from 'eslint-plugin-react-refresh'
import tseslint from 'typescript-eslint'
import { defineConfig, globalIgnores } from 'eslint/config'

export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      js.configs.recommended,
      tseslint.configs.recommended,
      reactHooks.configs['recommended-latest'],
      reactRefresh.configs.vite,
    ],
    languageOptions: {
      ecmaVersion: 2020,
      globals: globals.browser,
    },
    rules: {
      // Allow intentionally-unused catch-block error vars + args prefixed with _.
      // Stops 30+ "error is defined but never used" false positives without losing
      // the useful version of the rule for regular locals/imports.
      '@typescript-eslint/no-unused-vars': [
        'error',
        {
          argsIgnorePattern: '^_',
          varsIgnorePattern: '^_',
          caughtErrors: 'none',
        },
      ],
      // `any` is used in a handful of catch blocks and third-party payload shims.
      // Downgrade from error to warn so it's visible but non-blocking.
      '@typescript-eslint/no-explicit-any': 'warn',
      // This rule only affects Fast Refresh quality in dev (HMR may full-reload
      // instead of hot-updating). shadcn/ui components legitimately co-export
      // variant objects + components in the same file. Non-critical in prod.
      'react-refresh/only-export-components': 'warn',
    },
  },
])
