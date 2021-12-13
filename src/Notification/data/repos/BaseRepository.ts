import { Collection, Db, ObjectId } from "mongodb";
import IRepository from "../IRepository";
export abstract class BaseRepository<T> implements IRepository<T> {

    private _collection: Collection;

    constructor(mongo: Db, collectionName: string) {
        this._collection = mongo.collection(collectionName);
    }
    async Single(predicate?: string): Promise<T | undefined | null> {
        let result = (predicate != undefined) ?
            await this._collection.findOne<T>({
                _id: new ObjectId(predicate)
            }) :
            await this._collection.findOne<T>();
        return result;
    }
    async List(predicate?: Array<string>, skip: number = 0, lastId: string = '-1', take: number = -1): Promise<T[]> {
        let resdata: T[] = [];
        let data = {
        }
        if (predicate != undefined && predicate.length > 0)
            data = {
                id: {
                    $in: predicate.map(x => new ObjectId(x))
                }
            }
        if (predicate != undefined && predicate.length > 0 && lastId != '-1') {
            data = {
                id: {
                    $in: predicate.map(x => new ObjectId(x)),
                    $gt: new ObjectId(lastId)
                }
            }
        }
        let result = this._collection.find<T>(data)
        if (skip > 0) result.skip(skip);
        if (take > -1) result.limit(take);

        return new Promise((resolve, _) => {
            result.forEach(elem => {
                resdata.push(elem)
            })
            resolve(resdata)
        });
    }
    async Insert(entity: T): Promise<T | string> {
        let result = await this._collection.insertOne(entity)
        return result.insertedId.toString();
    }
    async Update(id: string, entity: T): Promise<T | string | null | undefined> {
        let result = await this._collection.updateOne({
            _id: new ObjectId(id)
        }, entity)
        return result.upsertedId.toString();
    }
    async Delete(id: string): Promise<T | number | null | undefined> {
        let result = await this._collection.deleteOne({
            _id: new ObjectId(id)
        })
        return result.deletedCount
    }
}