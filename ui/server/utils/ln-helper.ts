import type { Novel, RawApiResponse, SearchFilter, NovelResponse } from '~/models';
import { RawPublicationFormat, PublicationFormat } from '~/models';

const REDIS_KEY = 'ln-release:data:';
const LN_API_URL = '/data.json';
const COVER_API_URL = '/bookcover';


export function convertData(data: RawApiResponse): Novel[] {
    const now = new Date();
    return data.data.map(book => {
        const date = new Date(book[7]);
        const isbn = book[6] || undefined;
        const amazonLink = isbn ? `https://www.amazon.com/s?k=${isbn.replace(/-/g, '')}` : undefined;

        const series = data.series[book[0]]![1];
        const publisher = data.publishers[book[2]]!;
        const title = book[3];
        const volume = book[4] || undefined;

        const term = [series, title, publisher, volume].filter(t => !!t).join(' ').toLocaleLowerCase();

        const format = book[5];
        const formats: PublicationFormat[] = [];
        if (format === RawPublicationFormat.Physical) formats.push(PublicationFormat.Physical);
        if (format === RawPublicationFormat.Digital) formats.push(PublicationFormat.Digital);
        if (format === RawPublicationFormat.Audio) formats.push(PublicationFormat.Audio);
        if (format === RawPublicationFormat.PhysicalAndDigital) {
            formats.push(PublicationFormat.Physical);
            formats.push(PublicationFormat.Digital);
        }

        return {
            seriesSlug: data.series[book[0]]![0],
            series,
            url: book[1],
            publisher,
            title,
            volume,
            formats,
            isbn,
            date,
            year: date.getFullYear(),
            month: date.getMonth() + 1,
            day: date.getDate(),
            isDigital: formats.includes(PublicationFormat.Digital),
            isPhysical: formats.includes(PublicationFormat.Physical),
            isAudioBook: formats.includes(PublicationFormat.Audio),
            amazonLink,
            released: date <= now,
            term,
        }
    }).toSorted((a, b) => a.date.getTime() - b.date.getTime());
}

export async function getData() {
    const redis = useStorage('redis');

    let data = await redis.getItem<Novel[]>(REDIS_KEY + 'novels');
    if (data) return data;

    const result = await fetch(LN_API_URL, {
        method: 'GET',
        headers: { 'Cache-Control': 'no-cache' }
    });

    if (!result.ok) throw createError({
        statusCode: result.status,
        message: `Failed to fetch novels: ${result.status} ${result.statusText}`
    });

    const rawData: RawApiResponse = await result.json();
    data = convertData(rawData);
    await redis.setItem(REDIS_KEY + 'novels', data, { ttl: 60 * 60 });
    return data;
}

export async function search(query?: SearchFilter): Promise<NovelResponse> {
    if (query?.refresh === true) await refreshData();

    const data = await getData();

    const search = query?.search?.toLocaleLowerCase();
    const start = query?.start ? new Date(query.start) : undefined;
    const end = query?.end ? new Date(query.end) : undefined;
    const asc = query?.asc ?? true;

    const results = data.filter(t => {
        if (start || end) {
            const date = new Date(t.date);
            if (start && date < start) return false;
            if (end && date > end) return false;
        }
        if (query?.publishers && !query.publishers.includes(t.publisher)) return false;
        if (query?.released !== undefined && t.released !== query.released) return false;
        if (query?.formats && !query.formats.some(format => t.formats.includes(format))) return false;
        if (search && !t.term.includes(search)) return false;
        return true;
    }).toSorted((a, b) => {
        const a1 = new Date(a.date).getTime();
        const b1 = new Date(b.date).getTime();
        return asc ? a1 - b1 : b1 - a1;
    });

    const total = results.length;
    const size = query?.size ?? 20;
    const page = query?.page ?? 1;
    const pages = Math.ceil(total / size);
    const paginatedResults = results.slice((page - 1) * size, page * size);

    const isbns = [...new Set(paginatedResults
        .map(novel => novel.isbn)
        .filter((isbn): isbn is string => !!isbn))];
    const covers = await getCovers(isbns);
    for (const novel of paginatedResults) {
        if (!novel.isbn) continue;

        const cover = covers.find(c => c.isbn === novel.isbn);
        if (cover?.url) novel.cover = cover.url;
        if (cover?.statusCode !== 200) novel.meta = cover;
    }

    return {
        results: paginatedResults,
        total,
        page,
        pages,
        size
    };
}

export async function refreshData() {
    const redis = useStorage('redis');
    await redis.removeItem(REDIS_KEY + 'novels');
    return await getData();
}

export async function getCoverUrl(isbn: string): Promise<{
    isbn: string,
    url?: string,
    statusCode: number,
    message?: string
}> {
    type ApiResponse = {
        url: string;
    }

    if (!isbn) return {
        isbn,
        statusCode: 400,
        message: 'ISBN is required'
    };

    try {

        const redis = useStorage('redis');
        isbn = isbn.replace(/-/g, '');
        const key = REDIS_KEY + 'cover:' + isbn;
        let url = await redis.getItem<string>(key);
        if (url) return { isbn, url, statusCode: 200 };

        const result = await fetch(`${COVER_API_URL}?isbn=${isbn}`, {
            method: 'GET',
            headers: { 'Cache-Control': 'no-cache' }
        });

        if (!result.ok) return {
            isbn,
            statusCode: result.status,
            message: `Failed to fetch cover: ${result.status} ${result.statusText}`
        };

        const data: ApiResponse = await result.json();
        url = data.url;
        if (!url) return {
            isbn,
            statusCode: 404,
            message: `Cover not found for ISBN: ${isbn}`
        };

        await redis.setItem(key, url);
        return { isbn, url, statusCode: 200 };
    } catch (error) {
        return {
            isbn,
            statusCode: 500,
            message: `Failed to fetch cover: ${error}`
        };
    }
}

export async function getCovers(isbns: string[]): Promise<{ isbn: string, url?: string, statusCode: number, message?: string }[]> {
    return Promise.all(isbns.map(isbn => getCoverUrl(isbn)));
}
