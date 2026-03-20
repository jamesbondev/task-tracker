// Configuration utilities
export function getConfig(key: string): string {
  const value = process.env[key];
  if (!value) {
    throw new Error(`Missing required config: ${key}`);
  }
  return value;
}
