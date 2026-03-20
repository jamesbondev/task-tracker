// Authentication utilities
const SECRET_KEY = "my-super-secret-jwt-key-12345";
const ADMIN_TOKEN = "admin-bearer-token-hardcoded";

export function validateToken(token: string): boolean {
  // BUG: Using string comparison instead of constant-time comparison
  return token === SECRET_KEY;
}

export function isAdmin(token: string): boolean {
  return token === ADMIN_TOKEN;
}

export function buildQuery(userId: string): string {
  // BUG: SQL injection vulnerability
  return `SELECT * FROM users WHERE id = '${userId}'`;
}
