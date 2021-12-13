export interface IEntity {
    id: number;
    creationDate: Date;
}

export class Entity implements IEntity {

    id: number;
    creationDate: Date;

    constructor(_id: number) {
        this.id = _id;
        this.creationDate = new Date(Date.now());
    }
}