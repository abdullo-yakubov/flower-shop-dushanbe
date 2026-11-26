import type { SyntheticEvent } from 'react'

export const PLACEHOLDER_IMAGE = '/placeholder-flower.svg'

export function isCatalogIllustration(src?: string) {
  return !!src?.startsWith('/flowers/') && src.endsWith('.jpg')
}

export function catalogImageSources(src?: string) {
  if (!src || !isCatalogIllustration(src)) return undefined
  const base = src.replace(/\.jpg$/, '')
  // Варианты лежат рядом с оригиналом: -sm 352px, -md 680px, оригинал 1086px
  return `${base}-sm.jpg 352w, ${base}-md.jpg 680w, ${src} 1086w`
}

export function showImagePlaceholder(event: SyntheticEvent<HTMLImageElement>) {
  const image = event.currentTarget
  if (image.getAttribute('src') !== PLACEHOLDER_IMAGE) {
    image.removeAttribute('srcset')
    image.src = PLACEHOLDER_IMAGE
  }
}

/** Миниатюра (≤ 96px): для иллюстраций каталога берём вариант 352px вместо оригинала */
export function thumbImage(src?: string) {
  return src && isCatalogIllustration(src) ? src.replace(/\.jpg$/, '-sm.jpg') : src
}
