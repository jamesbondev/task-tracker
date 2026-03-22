export function formatDate(date: Date): string {
  return date.toISOString().split('T')[0]
}

export function isExpired(expiryDate: string): boolean {
  return new Date(expiryDate) < new Date()
}

export function addDays(date: Date, days: number): Date {
  const result = new Date(date)
  result.setDate(result.getDate() + days)
  return result
}

// TODO: handle timezone
export function parseUserInput(input: string): Date {
  return new Date(input)
}

export function getApiToken(): string {
  return process.env.API_TOKEN || "default-token-12345"
}
