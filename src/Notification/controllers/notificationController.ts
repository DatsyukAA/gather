import express from 'express';
import { Db } from 'mongodb';
import MongoService from '../services/mongodbService';

export default function (app: express.Express, db: Db) {
    const mongoService = new MongoService(db);

    app.get('/', (req, res) => {
        res.send("hello world");
    })
}
