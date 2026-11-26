import { describe, it, expect } from 'vitest'
import { existsSync } from 'node:fs'
import { resolve } from 'node:path'
import { catalogImageSources, isCatalogIllustration, thumbImage } from '@/shared/lib/image'
import { MOCK_PRODUCTS } from '@/entities/product/mock'

const publicPath = (url: string) => resolve(__dirname, '../../../../public', url.replace(/^\//, ''))

describe('image helpers', () => {
  it('srcset для иллюстрации каталога содержит три ширины', () => {
    expect(catalogImageSources('/flowers/red.jpg')).toBe(
      '/flowers/red-sm.jpg 352w, /flowers/red-md.jpg 680w, /flowers/red.jpg 1086w',
    )
  })

  it('внешние фото не получают srcset и не подменяются миниатюрой', () => {
    const url = 'https://cdn.example.com/a.jpg'
    expect(isCatalogIllustration(url)).toBe(false)
    expect(catalogImageSources(url)).toBeUndefined()
    expect(thumbImage(url)).toBe(url)
    expect(thumbImage(undefined)).toBeUndefined()
  })

  it('миниатюра указывает на -sm вариант', () => {
    expect(thumbImage('/flowers/tulip.jpg')).toBe('/flowers/tulip-sm.jpg')
  })

  it('все файлы из srcset демокаталога существуют в public/', () => {
    const urls = new Set<string>(['/flowers/hero.jpg'])
    for (const p of MOCK_PRODUCTS) for (const img of p.images) if (isCatalogIllustration(img)) urls.add(img)
    expect(urls.size).toBeGreaterThan(1)
    const missing: string[] = []
    for (const url of urls) {
      const srcset = catalogImageSources(url) ?? ''
      for (const candidate of srcset.split(',').map((s) => s.trim().split(' ')[0])) {
        if (!existsSync(publicPath(candidate))) missing.push(candidate)
      }
    }
    expect(missing).toEqual([])
  })
})
