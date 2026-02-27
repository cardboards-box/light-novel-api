import filesaver from 'file-saver';
import type { AsyncData } from '#app';
import type { FetchError } from 'ofetch';
import type {
    CacheResolver, ApiResult,
    BoxedBase, BoxedEmpty, BoxedError, BoxedArray, BoxedPaged, Boxed,

    RespMetaImage, RespMetaFormats, RespMetaPublisher,
    RespNovelsCalendar, RespNovelsRefresh, RespNovelsSearch,
    SearchFilter, SearchFilterBase,
    LncEntity,
    LncPublication,
    LncFullPublication
} from "../models";

export type Params = { [key: string]: any; };

type NuxtApiHandle = ReturnType<typeof useNuxtApiHelper>;

export const useNuxtApiHelper = () => {
    const { apiUrl, token, wrapUrl } = useSettingsHelper();

    const headers = () => {
        const t = token.value;
        if (!t) return undefined;
        return { 'Authorization': `Bearer ${t}` };
    };
    const clone = <T>(item: T) => <T>JSON.parse(JSON.stringify(item));

    function get<T>(url: string, params?: Params, lazy: boolean = false) {
        const opts = <any>{
            params,
            headers: headers()
        };

        if (lazy)
            return useLazyFetch<T>(wrapUrl(apiUrl, url), opts);

        return useFetch<T>(wrapUrl(apiUrl, url), opts);
    }

    function post<T>(url: string, body?: any, params?: Params, lazy: boolean = false) {
        const opts = <any>{
            params,
            headers: headers(),
            body,
            method: 'post'
        };

        if (lazy)
            return useLazyFetch<T>(wrapUrl(apiUrl, url), opts);

        return useFetch<T>(wrapUrl(apiUrl, url), opts);
    }

    function put<T>(url: string, body?: any, params?: Params, lazy: boolean = false) {
        const opts = <any>{
            params,
            headers: headers(),
            body,
            method: 'PUT'
        };

        if (lazy)
            return useLazyFetch<T>(wrapUrl(apiUrl, url), opts);

        return useFetch<T>(wrapUrl(apiUrl, url), opts);
    }

    function deletefn<T>(url: string, params?: Params, lazy: boolean = false) {
        const opts = <any>{
            params,
            headers: headers(),
            method: 'DELETE'
        };

        if (lazy)
            return useLazyFetch<T>(wrapUrl(apiUrl, url), opts);

        return useFetch<T>(wrapUrl(apiUrl, url), opts);
    }

    async function download(uri: string, name?: string, req?: RequestInit) {
        try {
            if (token.value) {
                const headers = {
                    'Authorization': `Bearer ${token.value}`
                }
                req ??= {
                    headers
                }
            }

            const response = await fetch(uri, req);
            const fileData = await response.blob();
            filesaver.saveAs(fileData, name);
        } catch (error) {
            console.error(`Failed to download ${uri}`, {
                error,
                uri,
                name,
                req
            });
            window.open(uri, '_blank')?.focus();
        }
    }


    return {
        get,
        post,
        postFile: <T>(url: string, file: File, param?: Params) => {
            const data = new FormData();
            data.append('file', file);
            return post<T>(url, data, param);
        },
        put,
        del: deletefn,
        download,
        clone,
        wrapUrl
    }
}

type FetchApiHandle = ReturnType<typeof useFetchApiHelper>;

export function useFetchApiHelper(cache: CacheResolver) {
    const { wrapUrl } = useSettingsHelper();

    const headers = (body?: any) => {

        const h: Record<string, string> = {
            'Accept': 'application/json',
        };

        if (!(body instanceof FormData)) {
            h['Content-Type'] = 'application/json';
        }

        const token = cache()?.token;
        if (token) h['Authorization'] = `Bearer ${token}`;
        return h;
    }

    const doReq = async <T extends BoxedBase = BoxedEmpty>(
        url: string,
        param: Params | undefined,
        method: string,
        opts?: RequestInit
    ): Promise<ApiResult<T>> => {
        try {
            const result = await fetch(wrapUrl(cache()?.apiUrl, url, param), {
                method: method,
                headers: headers(opts?.body),
                ...opts
            });

            if (!result.ok) {
                return await result.json() as ApiResult<T>;
            }

            return await result.json() as ApiResult<T>;
        } catch (ex) {
            console.error('Error occurred while fetching API:', {
                url,
                param,
                method,
                error: ex
            });
            return {
                type: 'error',
                code: 500,
                description: ex?.toString() || 'Internal Server Error',
                errors: [ex?.toString() || 'Internal Server Error'],
                elapsed: 0,
                requestId: 'client-error',
            };
        }
    }

    const get = <T extends BoxedBase = BoxedEmpty>(url: string, param?: Params) => doReq<T>(url, param, 'GET');
    const post = <T extends BoxedBase = BoxedEmpty>(url: string, body?: any, param?: Params) => doReq<T>(url, param, 'POST', { body: JSON.stringify(body) });
    const put = <T extends BoxedBase = BoxedEmpty>(url: string, body?: any, param?: Params) => doReq<T>(url, param, 'PUT', { body: JSON.stringify(body) });
    const del = <T extends BoxedBase = BoxedEmpty>(url: string, body?: any, param?: Params) => doReq<T>(url, param, 'DELETE', { body: JSON.stringify(body) });
    const postFile = <T extends BoxedBase = BoxedEmpty>(url: string, file: File, param?: Params) => {
        const formData = new FormData();
        formData.append('file', file);
        return doReq<T>(url, param, 'POST', { body: formData });
    }

    return { get, post, put, del, postFile, wrapUrl }
}

type Handles = NuxtApiHandle | FetchApiHandle;

type ApiReturnResults<M, T extends BoxedBase> =
    M extends FetchApiHandle ? Promise<ApiResult<T>> :
    M extends NuxtApiHandle ? AsyncData<ApiResult<T> | undefined, FetchError<BoxedError> | undefined> :
    never;

export function useSharedApiHelper<Handle extends Handles>(api: Handle) {
    const { wrapUrl, apiUrl } = useSettingsHelper();

    const { get: fnGet, post: fnPost, del: fnDel, put: fnPut, postFile: fnPostFile } = api;
    type ResultType<T extends BoxedBase> = ApiReturnResults<Handle, T>;
    type GetFunction = <T extends BoxedBase>(url: string, param?: Params) => ResultType<T>;
    type DataFunction = <T extends BoxedBase>(url: string, body?: any, param?: Params) => ResultType<T>;

    const get: GetFunction = <any>fnGet,
        post: DataFunction = <any>fnPost,
        put: DataFunction = <any>fnPut,
        del: DataFunction = <any>fnDel,
        postFile: DataFunction = <any>fnPostFile;

    return {
        image: {
            meta: (id: string) => get<RespMetaImage>(`/image/${id}/meta`),
            url: (id: string) => wrapUrl(apiUrl, `/image/${id}`)
        },
        meta: {
            publishers: () => get<RespMetaPublisher>('/meta/publishers'),
            formats: () => get<RespMetaFormats>('/meta/formats')
        },
        novels: {
            search: (filter: SearchFilter) => post<RespNovelsSearch>('/novels', filter),
            searchUrl: (filter: SearchFilter) => get<RespNovelsSearch>('/novels', filter),
            refresh: () => get<RespNovelsRefresh>('/novels/refresh'),
            calendar: {
                week: (date: Date | string, filter?: SearchFilterBase) => {
                    date = typeof date === 'string' ? new Date(date) : date;
                    const year = date.getFullYear();
                    const month = date.getMonth() + 1;
                    const day = date.getDate();
                    return get<RespNovelsCalendar>(`/novels/calendar/${year}-${month}-${day}/week`, filter);
                },
                month: (date: Date | string, filter?: SearchFilterBase) => {
                    date = typeof date === 'string' ? new Date(date) : date;
                    const year = date.getFullYear();
                    const month = date.getMonth() + 1;
                    const day = date.getDate();
                    return get<RespNovelsCalendar>(`/novels/calendar/${year}-${month}-${day}/month`, filter);
                }
            }
        }
    }
}

type ApiResponse<T> = BoxedError | BoxedEmpty | BoxedArray<T> | BoxedPaged<T> | Boxed<T>;

type ExplodeKey<T extends LncEntity<any, any>> = T['related'][number]['type'];

type RelatedByKey<
  T extends LncEntity<any, any>,
  K extends ExplodeKey<T>
> = Extract<T["related"][number], { type: K }>;

type DataByKey<
  T extends LncEntity<any, any>,
  K extends ExplodeKey<T>
> = NonNullable<RelatedByKey<T, K>["data"]>;


export function useApi() {
    const { apiUrl, token, wrapUrl } = useSettingsHelper();
    const conf = useRuntimeConfig();
    const fetch = useSharedApiHelper(useFetchApiHelper(() => ({ apiUrl, token: token.value, prod: conf.public.prod })));
    const nuxt = useSharedApiHelper(useNuxtApiHelper());

    function data<T>(response: BoxedError | BoxedEmpty): undefined;
    function data<T>(response: BoxedError | BoxedArray<T>): T[];
    function data<T>(response: BoxedError | BoxedPaged<T>): { data: T[]; total: number; pages: number };
    function data<T>(response: BoxedError | Boxed<T>): T;
    function data<T>(response: BoxedError | ApiResponse<T>) {
        if (!response) return undefined;
        if (response.type === 'paged') {
            return { data: response.data, total: response.total, pages: response.pages };
        }

        if ('data' in response)
            return response.data;

        return undefined;
    }

    /**
     * Gets the data from the related items
     * @param item The item containing the related data
     * @param key The key of the related data
     * @returns The related data or undefined if not found
     */
    function getRelated<
        T extends LncEntity<any, any>,
        const K extends ExplodeKey<T>
    >(item: T, key: K): DataByKey<T, K> | undefined {
        return item.related.find(r => r.type === key)?.data;
    }

    /**
     * Checks if the related item is of the specified type
     * @param key The key of the related data
     * @returns A type guard function
     */
    function isRelatedType<
        T extends LncEntity<any, any>,
        K extends ExplodeKey<T>
    >(key: K) {
        return (r: T["related"][number]): r is RelatedByKey<T, K> => r.type === key;
    }

    /**
     * Gets all of the related data of the specified type
     * @param item The item containing the related data
     * @param key The key of the related data
     * @returns The related data
     */
    function getRelateds<
        T extends LncEntity<any, any>,
        const K extends ExplodeKey<T>
    >(item: T, key: K): DataByKey<T, K>[] {
        return item.related
            .filter(isRelatedType<T, K>(key))
            .map(r => r.data)
            .filter((d): d is DataByKey<T, K> => d != null);
    }

    function amazonLink(isbn?: string | LncPublication | LncFullPublication) {
        if (!isbn) return undefined;

        if (typeof isbn !== 'string' && 'entity' in isbn) isbn = isbn.entity;
        if (typeof isbn !== 'string' && 'id' in isbn) isbn = isbn.isbn;

        if (!isbn) return undefined;

        return `https://www.amazon.com/s?k=${isbn}`;
    }

    return {
        promise: fetch,
        nuxt: nuxt,
        isSuccess: <T extends BoxedBase>(response?: T) => {
            if (!response) return false;
            const code = response.code ?? 500;
            return code >= 200 && code < 300;
        },
        errorMessage: <T extends BoxedBase>(response?: T) => {
            if (!response)
                return 'No response';

            const code = response.code ?? 500;
            if (code >= 200 && code < 300)
                return undefined;

            const errors = response.errors ?? ['Unknown error'];
            let message = '';
            if (response.description) {
                message = response.description + ". ";
            }
            message += errors.join('; ');
            return message;
        },
        getRelated,
        getRelateds,
        data,
        amazonLink,
        wrapUrl: (url: string) => wrapUrl(apiUrl, url)
    }
}
