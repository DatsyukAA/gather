import { EEventType, IChatMessage, IWSSchema, WsClient } from "./types";
import Notification from "../data/entities/Notification";
import INotificationService from "../services/INotificationService";

class WSEventHandler {
  constructor(
    private clients: WsClient[],
    private sender: WsClient,
    private service: INotificationService
  ) { 
    service.subscribe((notification: Notification) => {
        this.send({
          data: notification,
          type: EEventType.MESSAGE
        }, [sender.userId])
    },sender.userId.toString())
  }

  resolve(schema: IWSSchema) {
    switch (schema.type) {
      case EEventType.MESSAGE:
        this.onChatMessage(schema.data);
        break;
    }
  }

  onChatMessage = (chatMessage: IChatMessage) => {
    this.service.notify(
      new Notification(this.sender.userId.toString(), chatMessage.title ?? '', chatMessage.message ?? ''), 
      chatMessage.channelId)
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
