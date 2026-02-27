type RawSeries = [string, string];
type RawData = [number, string, number, string, string, RawPublicationFormat, string, string];

export interface RawApiResponse {
    series: RawSeries[];
    publishers: string[];
    data: RawData[];
}

/** The various publication formats a novel can have */
export enum RawPublicationFormat {
    /** Physical copy of the novel */
    Physical = 1,
    /** Digital copy of the novel */
    Digital = 2,
    /** Both physical and digital copies of the novel */
    PhysicalAndDigital = 3,
    /** Audiobook version of the novel */
    Audio = 4,
}

/** The various publication formats a novel can have */
export enum PublicationFormat {
    /** Physical copy of the novel */
    Physical = 0,
    /** Digital copy of the novel */
    Digital = 1,
    /** Audiobook version of the novel */
    Audio = 2
}

/** Represents a novel in the system */
export interface Novel {
    /** The slug of the series the novel belongs to */
    seriesSlug: string;
    /** The series the novel belongs to */
    series: string;
    /** The URL to the novel's home page */
    url: string;
    /** The publisher of the novel */
    publisher: string;
    /** The title of the novel */
    title: string;
    /** The volume number of the novel, if applicable */
    volume?: string;
    /** The publication formats of the novel */
    formats: PublicationFormat[];
    /** The ISBN of the novel, if applicable */
    isbn?: string;
    /** The release date of the novel */
    date: Date | string;
    /** The year the novel was released */
    year: number;
    /** The month the novel was released */
    month: number;
    /** The day the novel was released */
    day: number;
    /** Whether or not the novel has a digital version */
    isDigital: boolean;
    /** Whether or not the novel has a physical version */
    isPhysical: boolean;
    /** Whether or not the novel is an audiobook */
    isAudioBook: boolean;
    /** The Amazon link for the novel, if applicable */
    amazonLink?: string;
    /** Whether or not the novel has been released */
    released: boolean;
    /** The search term associated with the novel */
    term: string;
    /** The cover image URL for the novel, if available */
    cover?: string;
    /** The meta-data for the cover response if it returned an error */
    meta?: {
        /** The ISBN of the novel for which the cover response was returned */
        isbn: string;
        /** The URL of the cover image, if available */
        url?: string;
        /** The HTTP status code returned by the cover API */
        statusCode: number;
        /** The error message returned by the cover API, if any */
        message?: string;
    };
}

/** The response from the novels API */
export interface NovelResponse {
    /** The novels returned by the API */
    results: Novel[];
    /** The total number of novels matching the search criteria */
    total: number;
    /** The current page number */
    page: number;
    /** The total number of pages available */
    pages: number;
    /** The number of results per page */
    size: number;
}

/** The url parameters for searching */
export interface SearchFilter {
    /** The start range for the filter */
    start?: Date;
    /** The end range for the filter */
    end?: Date;
    /** All of the publishers to filter by */
    publishers?: string[];
    /** Whether or not to filter by released status */
    released?: boolean;
    /** The publication formats to filter by */
    formats?: PublicationFormat[];
    /** The search term to filter by */
    search?: string;
    /** Whether or not to sort in ascending order */
    asc?: boolean;
    /** Whether or not to refresh the data */
    refresh?: boolean;
    /** The maximum number of results to return */
    size?: number;
    /** The page number to return */
    page?: number;
}

/** The server-side query parameters for searching novels on the server */
export type SearchFilterServer = {
    [key in keyof SearchFilter]: string | string[] | undefined;
}
