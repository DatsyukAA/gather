import WebSocket from "ws";
import WSEventHandler from "./WSEventHandler";
import { EEventType, IWSSchema, WsClient } from "./types";
import { Db } from "mongodb";
import * as http from "http";
import * as url from "url";
import jwt from "jsonwebtoken";
import { config } from "../appsettings";
import aes from "../aes/aes";

class WSHandler {
  clients: WsClient[] = [];

  constructor(wsServer: WebSocket.Server, private db: Db) {
    this.subscribe(wsServer);
  }

  parseJwt = (token: string) => {
    const base64Url = token.split(".")[1];
    const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
    const buff = new Buffer(base64, "base64");
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
          this.db
        ).resolve(wsData);
      });
    });
  };

  onConnect = (socket: WebSocket, req: http.IncomingMessage) => {
    const queryParams = url.parse(req.url || "", true).query;
    console.log("client trying connect");
    if (queryParams.access_token === undefined) {
      this.clients = this.clients.filter((x) => {
        x.socket !== socket;
      });
      socket.close(4000, "Not authorized");
      return;
    }
    //if gateway doesn't decrypt key or it passes through get/post param
    const token = aes.Decrypt(queryParams.access_token?.toString());
    jwt.verify(
      token,
      Buffer.from(config.jwt.secret, "base64"), //throw depricated Buffer() when using string, but i actually have base64 encoded secret in token and it works fine
      {
        algorithms: ["HS256"],
      },
      (err, decoded: any) => {
        if (!!err || decoded === undefined || decoded.id === undefined) {
          console.log("Not authorized");
          return;
        }
        this.clients.forEach((element) => {
          element.socket.send(
            JSON.stringify({
              type: EEventType.CONNECT,
              data: {
                id: decoded.id,
              },
            })
          );
        });
        console.log(`User connected`);
        this.clients.push({
          socket,
          userId: decoded.id,
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
