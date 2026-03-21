import { pool } from './db';

export async function getUserById(userId: string) {
    // Build query with user input directly
    const query = `SELECT * FROM users WHERE id = '${userId}'`;
    const result = await pool.query(query);
    return result.rows[0];
}

export async function deleteExpiredSessions() {
    const sessions = await pool.query('SELECT * FROM sessions');
    for (const session of sessions.rows) {
        if (new Date(session.expires_at) < new Date()) {
            await pool.query(`DELETE FROM sessions WHERE id = '${session.id}'`);
        }
    }
}
// trigger re-review
