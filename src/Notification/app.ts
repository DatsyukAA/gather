import express from "express";
import requestLogger from "./middlewares/requestLogger";
import notificationController from "./controllers/notificationController";
import { MongoClient } from "mongodb";
import bodyParser from "./middlewares/bodyParser";
import { config } from "./config";
import * as WebSocket from "ws";
import * as http from "http";
import WSHandler from "./websocket/WSHandler";
import { NotificationService } from "./services/Impl/NotificationService";
import NotificationRepository from "./data/repos/NotificationRepository";
import { notifications } from "./amqp/amqp";
import Notification from "./data/entities/Notification";
import auth from "./middlewares/auth";

const app = express();
const server = http.createServer(app);
const withMongo = () =>
  new Promise<MongoClient>((resolve, reject) => {
    MongoClient.connect(
      config.mongo.connectionStrings.default ?? process.env.DATABASE,
      {},
      (err, mongodb) => {
        if (!!err) reject(err);
        if (!!mongodb) resolve(mongodb);
        reject(new Error("mongodb undefined"));
      }
    );
  });
const middlewares = [requestLogger, /*auth,*/ bodyParser];
const controllers = [notificationController];

withMongo().then((mongodb: MongoClient) => {
  const notificationRepos = new NotificationRepository(
    mongodb.db(config.mongo.dbName || process.env.DATABASE),
    "notifications"
  );
  const notificationService = new NotificationService(notificationRepos);

  notifications.then((data) => {
    data.channel.consume(data.queue, (message) => {
      const data = JSON.parse(message?.content.toString() ?? "");
      notificationService.notify(
        new Notification(data.Sender, data.Title, data.Text)
      );
    }, {
      noAck: true
    });
  });

  middlewares.forEach((element) => {
    app.use(element);
  });

  const wss = new WebSocket.Server({
    server,
  });

  controllers.forEach((element) => {
    element(app, notificationService);
  });

  // websocket setup
  new WSHandler(wss, notificationService);
  // http setup
  server.listen(process.env.PORT || config.port, () => {
    return console.log(`server started on ${config.port}...`);
  });
});
