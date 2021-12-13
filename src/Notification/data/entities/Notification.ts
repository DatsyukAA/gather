import { ObjectId } from 'bson';
import { Entity } from '../Entity'
export default class Notification extends Entity {
    title: string;
    text: string;
    sender: string;
    constructor(_sender: string, _title: string, _text: string) {
        super(new ObjectId().toString());
        this.title = _title;
        this.text = _text;
        this.sender = _sender;
    }
}