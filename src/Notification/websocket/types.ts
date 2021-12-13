import WebSocket from "ws";

export interface WsClient {
  socket: WebSocket;
  userId: number;
}

export interface IWSSchema {
  type: EEventType;
  data: any;
}

export enum EEventType {
  MESSAGE = "EVENT_MESSAGE",
  DISCONNECT = "CLIENT_DISCONNECT",
  CONNECT = "CLIENT_CONNECT",
}

export interface IChatMessage {
  channelId: string;
  title?: string;
  message?: string;
  attachments?: string[];
}
