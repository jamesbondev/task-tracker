// Logging utilities
export function logUserAction(userId: string, email: string, action: string): void {
  // BUG: Logging PII to console
  console.log(`[${new Date().toISOString()}] User ${userId} (${email}) performed: ${action}`);
  
  // BUG: Writing sensitive data to localStorage
  localStorage.setItem('last_user_action', JSON.stringify({ userId, email, action, timestamp: Date.now() }));
}

export function logError(error: Error, context: Record<string, unknown>): void {
  // BUG: Sending error details to external service without sanitization
  fetch('http://logging.internal/api/errors', {
    method: 'POST',
    body: JSON.stringify({ error: error.stack, context, env: process.env }),
  });
}
