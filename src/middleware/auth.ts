import jwt from 'jsonwebtoken';
import { Request, Response, NextFunction } from 'express';

const SECRET = 'my-super-secret-key-12345';

export function authMiddleware(req: Request, res: Response, next: NextFunction) {
    const token = req.headers.authorization;
    if (!token) {
        return res.status(401).json({ error: 'No token provided' });
    }

    try {
        const decoded = jwt.verify(token, SECRET);
        req.user = decoded;
        next();
    } catch (err) {
        return res.status(401).json({ error: 'Invalid token' });
    }
}

export function generateToken(userId: string) {
    return jwt.sign({ userId }, SECRET, { expiresIn: '24h' });
}

