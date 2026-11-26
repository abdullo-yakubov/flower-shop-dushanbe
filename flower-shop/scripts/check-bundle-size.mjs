/**
 * Bundle size gate — runs after `npm run build` in CI.
 * Fails if the total gzipped JS in dist/assets exceeds the budget.
 */

import { readFileSync, readdirSync, statSync } from 'fs'
import { resolve, dirname } from 'path'
import { fileURLToPath } from 'url'
import { gzipSync } from 'zlib'

const ROOT = resolve(dirname(fileURLToPath(import.meta.url)), '..')
const DIST = resolve(ROOT, 'dist/assets')

// Budget: total JS gzipped must stay under 350 KB
const BUDGET_KB = 350

const jsFiles = readdirSync(DIST)
  .filter((f) => f.endsWith('.js'))
  .map((file) => {
    const filePath = resolve(DIST, file)
    return {
      file,
      raw: statSync(filePath).size,
      gzip: gzipSync(readFileSync(filePath), { level: 9 }).length,
    }
  })
  .sort((a, b) => b.gzip - a.gzip)

console.log('Bundle size report:')
for (const { file, raw, gzip } of jsFiles) {
  console.log(
    `  ${file.padEnd(50)} ${(raw / 1024).toFixed(1).padStart(7)} KB raw  /  ${(gzip / 1024).toFixed(1).padStart(6)} KB gzip`
  )
}

const totalKb = jsFiles.reduce((s, f) => s + f.gzip, 0) / 1024
console.log(`\nTotal JS (gzip): ${totalKb.toFixed(1)} KB  /  budget: ${BUDGET_KB} KB`)

if (totalKb > BUDGET_KB) {
  console.error(`\x1b[31m✖ Bundle exceeds ${BUDGET_KB} KB budget (${totalKb.toFixed(1)} KB)\x1b[0m`)
  process.exit(1)
}
console.log(`\x1b[32m✔ Within budget\x1b[0m`)
