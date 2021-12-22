import WebSocket from "ws";
import WSEventHandler from "./WSEventHandler";
import { EEventType, IWSSchema, WsClient } from "./types";
import * as http from "http";
import * as url from "url";
import jwt from "jsonwebtoken";
import { NotificationService } from "../services/Impl/NotificationService";

class WSHandler {
  clients: WsClient[] = [];

  constructor(wsServer: WebSocket.Server, private _notifications: NotificationService) {
    this.subscribe(wsServer);
  }

  parseJwt = (token: string) => {
    const base64Url = token.split(".")[1];
    const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
    const buff = Buffer.from(base64, "base64");
    const payloadinit = buff.toString("ascii");
    const payload = JSON.parse(payloadinit);
    return payload;
  };

  subscribe = (server: WebSocket.Server) => {
    server.on("connection", (ws: WebSocket, req: http.IncomingMessage) => {
      this.onConnect(ws, req);

      ws.on("close", (socket: WebSocket, code: number, reason: string) => {
        const userConnection = this.clients.find((value) => {
          return value.socket === ws;
        });
        this.clients.filter((value) => {
          return !(value.socket == ws);
        });
        if (userConnection !== undefined) this.onDisconnect(userConnection);
      });

      // event resolver
      ws.on("message", (req: string) => {
        console.log("message received");
        const wsData = JSON.parse(req) as IWSSchema;
        new WSEventHandler(
          this.clients,
          this.clients.filter((element) => {
            return element.socket == ws;
          })[0],
          this._notifications
        ).resolve(wsData);
      });
    });
  };

  onConnect = (socket: WebSocket, req: http.IncomingMessage) => {
    const queryParams = url.parse(req.url || "", true).query;

    const token = queryParams.access_token?.toString();
    jwt.verify(
      token ?? '',
      Buffer.from(process.env?.jwt_secret ?? '', "base64"),
      {
        algorithms: ["HS256"],
      },
      (err: any, user: any) => {
        console.log(`User connected`);
        this._notifications.subscribe((data) => {
          socket.send(JSON.stringify({
            type: EEventType.MESSAGE,
            data: {
              sender: data.sender,
              title: data.title,
              date: data.creationDate,
              text: data.text
            },
          }))
        }, user?.id ?? 'public')
        this.clients.push({
          socket,
          userId: user?.id ?? 'public',
        });
      }
    );
  };

  onDisconnect = (client: WsClient) => {
    this.clients = this.clients.filter((element) => element !== client);
    this.clients.forEach((element) => {
      element.socket.send(
        JSON.stringify({
          type: EEventType.DISCONNECT,
          data: {
            id: client.userId,
          },
        })
      );
    });
    console.log(`User with id ${client.userId} disconnected`);
  };
}

export default WSHandler;
