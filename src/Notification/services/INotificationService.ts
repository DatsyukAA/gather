import Notification from '../data/entities/Notification'

export default interface INotificationService {
    subscribe(callback: Function, channelId?: string): void;
    unsubscribe(callback: Function, channelId?: string): void;
    notify(notification: Notification, channelId?: string): void;
}