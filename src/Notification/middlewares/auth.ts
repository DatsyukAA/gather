import express from 'express'
import jwt from "jsonwebtoken";

export default function auth(request: express.Request, response: express.Response, next: Function) {
    const token = request.headers.authorization?.split(' ')[1] ?? '';
    jwt.verify(
        token,
        Buffer.from(process.env?.jwt_secret ?? '', "base64"),
        {
            algorithms: ["HS256"],
        },
        (err: any, decoded: any) => {
            if (!!err || decoded === undefined || decoded.id === undefined) {
                response.statusCode = 401;
                response.send();
            }
            (request as any).user = decoded
            next();
        }
    );
}
