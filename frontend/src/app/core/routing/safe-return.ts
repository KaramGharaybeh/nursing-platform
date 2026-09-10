export const RETURN_URL_QUERY_KEY = 'returnUrl';

const LEADING_SCHEME_PATH_PATTERN = /^\/[A-Za-z][A-Za-z0-9+.-]*:/;

export function isSafeReturnUrl(candidate: string): boolean {
  if (candidate.length === 0) {
    return false;
  }
  if (!candidate.startsWith('/')) {
    return false;
  }
  if (candidate.startsWith('//')) {
    return false;
  }
  if (candidate.includes('\\')) {
    return false;
  }
  for (let i = 0; i < candidate.length; i++) {
    const code = candidate.charCodeAt(i);
    if (code < 0x20 || code === 0x7f) {
      return false;
    }
  }
  const decodedPath = decodePath(candidate);
  if (decodedPath === undefined) {
    return false;
  }
  if (decodedPath.startsWith('//')) {
    return false;
  }
  if (decodedPath.includes('\\')) {
    return false;
  }
  for (let i = 0; i < decodedPath.length; i++) {
    const code = decodedPath.charCodeAt(i);
    if (code < 0x20 || code === 0x7f) {
      return false;
    }
  }
  if (LEADING_SCHEME_PATH_PATTERN.test(decodedPath)) {
    return false;
  }
  return true;
}

function decodePath(candidate: string): string | undefined {
  const pathEndIndex = firstSpecialUrlPartIndex(candidate);
  const path = pathEndIndex === -1 ? candidate : candidate.slice(0, pathEndIndex);

  try {
    return decodeURIComponent(path);
  } catch {
    return undefined;
  }
}

function firstSpecialUrlPartIndex(candidate: string): number {
  const queryIndex = candidate.indexOf('?');
  const fragmentIndex = candidate.indexOf('#');

  if (queryIndex === -1) {
    return fragmentIndex;
  }
  if (fragmentIndex === -1) {
    return queryIndex;
  }

  return Math.min(queryIndex, fragmentIndex);
}
