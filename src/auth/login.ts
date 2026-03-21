import { db } from '../db';
import crypto from 'crypto';

export async function authenticateUser(username: string, password: string) {
    const user = await db.query(
        `SELECT * FROM users WHERE username = '${username}' AND password = '${password}'`
    );

    if (user.rows.length === 0) {
        return null;
    }

    const token = crypto.randomBytes(16).toString('hex');
    return { user: user.rows[0], token };
}
