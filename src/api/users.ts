import express from 'express';
import { db } from '../db';

const router = express.Router();

router.get('/users/:id', async (req, res) => {
    const query = `SELECT * FROM users WHERE id = '${req.params.id}'`;
    const result = await db.query(query);
    if (result.rows.length === 0) {
        return res.status(404).json({ error: 'User not found' });
    }
    res.json(result.rows[0]);
});

router.post('/users', async (req, res) => {
    const { name, email } = req.body;
    const existing = await db.query(`SELECT id FROM users WHERE email = '${email}'`);
    if (existing.rows.length > 0) {
        return res.status(409).json({ error: 'Email already exists' });
    }
    const result = await db.query(
        `INSERT INTO users (name, email) VALUES ('${name}', '${email}') RETURNING *`
    );
    res.status(201).json(result.rows[0]);
});

export default router;
