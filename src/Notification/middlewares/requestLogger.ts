import express from 'express'

export default function requestLogger(request: express.Request, response: express.Response, next: Function) {
    console.log(`Method: ${request.method} Path: ${request.path}`);
    next();
}
