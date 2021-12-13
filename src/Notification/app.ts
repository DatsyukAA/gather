import express from "express";
import requestLogger from "./middlewares/requestLogger";
import notificationController from "./controllers/notificationController";
import mongo from "mongodb";
import bodyParser from "./middlewares/bodyParser";
import { config } from "./config";
import * as WebSocket from "ws";
import * as http from "http";
import WSHandler from "./websocket/WSHandler"


const app = express();
const server = http.createServer(app);
const middlewares = [requestLogger, bodyParser];
const controllers = [notificationController];
middlewares.forEach((element) => {
    app.use(element);
});


const wss = new WebSocket.Server({
    server
});
mongo.MongoClient.connect(
    config.mongo.connectionStrings.default, {},
    (err, mongodb) => {
        if (err) return console.log(err);
        if (mongodb == undefined) return console.log('mongodb undefined')

        controllers.forEach((element) => {
            element(app, mongodb.db(config.mongo.dbName || process.env.DATABASE));
        });
        // websocket setup
        new WSHandler(wss, mongodb.db(config.mongo.dbName || process.env.DATABASE))
        // http setup
        server.listen(process.env.PORT || config.port, () => {
            return console.log(`server started on ${config.port}...`);
        });
    }
);
