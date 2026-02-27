import type { NovelResponse, SearchFilter } from '~/models';

export type Params = { [key: string]: any; };

export const useNovelSearch = (key: string, filter: Ref<SearchFilter | undefined>) => {
    const URL = '/api/data';

    const getParamValue = (value: any) => {
        if (value === undefined || value === null) return undefined;

        if (typeof value === 'boolean') return value ? 'true' : 'false';
        if (typeof value === 'string') return value ? value : undefined;
        if (value instanceof Date) return value.toISOString();

        return value.toString();
    }

    const addParams = (url: string, param?: Params | undefined) => {
        if (!param) return url;

        const parameters = [];
        for (const key in param) {
            if (param[key] === undefined || param[key] === null) continue;

            if (Array.isArray(param[key])) {
                for (const item of param[key]) {
                    parameters.push(`${encodeURIComponent(key)}=${encodeURIComponent(getParamValue(item)!)}`);
                }
                continue;
            }

            parameters.push(`${encodeURIComponent(key)}=${encodeURIComponent(getParamValue(param[key])!)}`);
        }

        if (url.indexOf('?') !== -1)
            url = url.substring(0, url.indexOf('?'));

        const ps = parameters.join('&');
        return `${url}${(!!ps ? '?' + ps : '')}`;
    }


    return useAsyncData<NovelResponse>(
        key,
        async () => {
            const url = addParams(URL, filter.value);
            const res = await fetch(url);
            if (!res.ok) throw new Error(`Failed to fetch novels: ${res.statusText}`);
            return await res.json() as NovelResponse;
        },
        { watch: [filter] }
    );
}

export const usePublishers = () => {
    const URL = '/api/publishers';
    return useAsyncData<string[]>(
        'publishers',
        async () => {
            const res = await fetch(URL);
            if (!res.ok) throw new Error(`Failed to fetch publishers: ${res.statusText}`);
            return await res.json() as string[];
        }
    );
}
