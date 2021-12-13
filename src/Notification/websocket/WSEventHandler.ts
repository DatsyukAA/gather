import { EEventType, IChatMessage, IWSSchema, WsClient } from "./types";
import { Db } from "mongodb";

class WSEventHandler {
  constructor(
    private clients: WsClient[],
    private sender: WsClient,
    private db: Db
  ) { }

  resolve(schema: IWSSchema) {
    switch (schema.type) {
      case EEventType.MESSAGE:
        this.onChatMessage(schema.data);
        break;
    }
  }

  onChatMessage = (chatMessage: IChatMessage) => {
    // check with filter
    this.db
      .collection("messages")
      .insertOne({
        userId: this.sender.userId,
        message: chatMessage,
      })
      .then((r) => {
        console.log(
          `element added. result = ${r.acknowledged == true ? "OK" : "Error"}`
        );
        this.send({
          type: EEventType.MESSAGE,
          data: chatMessage,
        });
      });
  };
  send = (message: IWSSchema, clients: number[] | undefined = undefined) => {
    let receivers = this.clients;
    if (clients !== undefined) {
      receivers = this.clients.filter((x) => {
        clients?.includes(x.userId);
      });
    }
    receivers.forEach((client) => {
      client.socket.send(JSON.stringify(message));
    });
  };
}

export default WSEventHandler;
