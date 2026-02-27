/**
 * The base interface for API responses
 */
export interface BoxedBase {
    /** The HTTP status code of the response */
    code: number;
    /** A breif description of the error */
    description?: string;
    /** Any issues that occurred */
    errors?: string[];
    /** The number of milliseconds the request took to complete */
    elapsed: number;
    /** The unique identifier for the request */
    requestId: string;
}

/**
 * The interface for API error responses
 */
export interface BoxedError extends BoxedBase {
    /** The type of the result */
    type: 'error';
}

/**
 * The interface for API responses with no data
 */
export interface BoxedEmpty extends BoxedBase {
    /** The type of the result */
    type: 'ok';
}

/**
 * The interface for API responses with data
 */
export interface Boxed<Data = any> extends BoxedBase {
    /** The type of the result */
    type: 'data';
    /** The data returned by the API */
    data: Data;
}

/**
 * The interface for API responses with an array of data
 */
export interface BoxedArray<Data = any> extends BoxedBase {
    /** The type of the result */
    type: 'array';
    /** The data returned by the API */
    data: Data[];
}

/**
 * The interface for API responses with paged data
 */
export interface BoxedPaged<Data = any> extends BoxedBase {
    /** The type of the result */
    type: 'paged';
    /** The data returned by the API */
    data: Data[];
    /** The total number of pages available */
    pages: number;
    /** The total number of items available */
    total: number;
}

/**
 * The interface for database objects
 */
export interface LncDbObject {
    /** The ID of the entity */
    id: string;

    /** The date the entity was created */
    createdAt: Date | string;
    /** The date the entity was last updated */
    updatedAt: Date | string;
    /** The date the entity was deleted */
    deletedAt?: Date | string;
}

export interface EnumDescription<TValue = number> {
    name: string;
    description: string;
    value: TValue;
    typeName?: string;
}

export interface ApiConfig {
    token: string | undefined;
    apiUrl: string;
    prod: boolean;
}

export type CacheResolver = () => ApiConfig | undefined;

export type ApiResult<T extends BoxedBase> = BoxedError | T;

export enum LncFormat {
    Physical = 0,
    Digital = 1,
    Audio = 2
}

export interface LncCover extends LncDbObject {
    isbn: string;
    coverUrl?: string;
    fileName?: string;
    urlHash?: string;
    imageWidth?: number;
    imageHeight?: number;
    imageSize?: number;
    mimeType?: string;
}

export interface LncPublication extends LncDbObject {
    volumeId: string;
    publisherId: string;
    format: LncFormat;
    isbn?: string;
    url?: string;
    releaseDate?: Date | string;
}

export interface LncPublisher extends LncDbObject {
    slug: string;
    name: string;
    iconUrl?: string;
    website?: string;
}

export interface LncSeries extends LncDbObject {
    slug: string;
    title: string;
}

export interface LncVolume extends LncDbObject {
    seriesId: string;
    volume: string;
    title: string;
}

export interface LncRelated<TType = string, TData = any> {
    type: TType;
    data?: TData;
}

type RelatedCover = LncRelated<'LncCover', LncCover>;
type RelatedPublication = LncRelated<'LncPublication', LncPublication>;
type RelatedPublisher = LncRelated<'LncPublisher', LncPublisher>;
type RelatedSeries = LncRelated<'LncSeries', LncSeries>;
type RelatedVolume = LncRelated<'LncVolume', LncVolume>;

export type LncRelationship = RelatedCover | RelatedPublication | RelatedPublisher | RelatedSeries | RelatedVolume;

export interface LncEntity<T, TRelated extends LncRelated = LncRelationship> {
    entity: T;
    related: TRelated[];
}

export type LncFullPublication = LncEntity<LncPublication, RelatedCover | RelatedPublisher | RelatedSeries | RelatedVolume>;

export interface CalendarResults<T> {
    start: Date | string;
    end: Date | string;
    chunk: number;
    entities: {
        [key: string]: T;
    };
    entries: {
        start: Date | string;
        end: Date | string;
        entries: {
            date: Date | string;
            entities: string[];
        }[]
    }[]
}

export interface SearchFilterBase {
    publisher?: string[];
    released?: boolean;
    format?: LncFormat[];
    search?: string;
    isbn?: string[];
    asc?: boolean;
}

export interface SearchFilter extends SearchFilterBase {
    page?: number;
    size?: number;
    start?: Date | string;
    end?: Date | string;
}

export interface RespMetaImage extends Boxed<LncCover> { }
export interface RespMetaPublisher extends BoxedArray<LncPublisher> { }
export interface RespMetaFormats extends BoxedArray<EnumDescription<LncFormat>> { }
export interface RespNovelsSearch extends BoxedPaged<LncFullPublication> { }
export interface RespNovelsCalendar extends Boxed<CalendarResults<LncFullPublication>> { }
export interface RespNovelsRefresh extends Boxed<{
    publishers: LncPublisher[];
    series: LncSeries[];
    volumes: LncVolume[];
    publications: LncPublication[];
}> { }
