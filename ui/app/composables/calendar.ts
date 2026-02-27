import type { LocationQueryValue } from 'vue-router';
import type { SearchFilterBase } from '~/models';
import { LncFormat } from '~/models';

type SearchFilter = SearchFilterBase & {
    date?: Date | string;
};

export function useCalendarHelper(weekly: boolean) {
    const api = useApi();
    const route = useRoute();
    const router = useRouter();
    const { throttle } = useUtils();
    const { addParams } = useSettingsHelper();

    const DEFAULT_FILTERS: SearchFilter = {
        date: new Date(),
        asc: true,
        search: '',
        format: [LncFormat.Digital, LncFormat.Audio],
        isbn: [],
    } as const;

    function parseFilters(): SearchFilter {
        const getArray = <T = string>(value: LocationQueryValue | LocationQueryValue[], func?: (v: string) => T): T[] => {
            const values =  Array.isArray(value)
                ? value.map(v => v?.toString()?.toLowerCase()!).filter(t => !!t)
                : value ? [value.toString().toLowerCase()!] : [];

            if (func) return values.map(func);
            return <any>values;
        }

        const filter = { ...DEFAULT_FILTERS };

        const pars: {
            [key: string]: {
                prop: keyof SearchFilter,
                massage?: (value: LocationQueryValue | LocationQueryValue[]) => any
            }
        } = {
            'publisher': { prop: 'publisher', massage: (v) => getArray(v) },
            'released': { prop: 'released', massage: (v) => v?.toString().toLocaleLowerCase() === 'true' },
            'format': { prop: 'format', massage: (v) => getArray(v, (val) => +val) },
            'search': { prop: 'search' },
            'isbn': { prop: 'isbn', massage: (v) => getArray(v) },
            'asc': { prop: 'asc', massage: (v) => v?.toString().toLocaleLowerCase() === 'true' },
            'date': { prop: 'date', massage: (v) => new Date(v?.toString()!) },
        };

        for(const key in route.query) {
            const param = pars[key.toLowerCase()];
            if (!param) continue;

            const value = route.query[key];
            if (!value) continue;

            filter[param.prop] = param.massage ? param.massage(value) : route.query[key];
        }

        return filter;
    }

    function setup() {
        const { pending: novPending, error: novError, data: novData, refresh } = useAsyncData(
            `calendar-${weekly ? 'weekly' : 'monthly'}-${route.fullPath}`,
            async () => {
                const filters = parseFilters();
                const date = filters.date ?? new Date();
                return await (weekly
                    ? api.promise.novels.calendar.week
                    : api.promise.novels.calendar.month)(date, filters);
            },
            { watch: [() => route.query ]}
        );
        const calendar = computed(() => (novData.value && api.data(novData.value)));

        const { pending: frmPending, error: frmError, data: frmData } = useAsyncData(
            'formats',
            async () => await api.promise.meta.formats()
        );
        const formats = computed(() => (frmData.value && api.data(frmData.value)) ?? []);

        const { pending: pubPending, error: pubError, data: pubData } = useAsyncData(
            'publishers',
            async () => await api.promise.meta.publishers()
        );
        const publishers = computed(() => ((pubData.value && api.data(pubData.value)) ?? []).map(t => ({ name: t.name, value: t.id })));
        const pending = computed(() => novPending.value || pubPending.value || frmPending.value);

        return {
            refresh: throttle<void>(() => { if (!pending.value) refresh(); }, 250),
            publishers,
            formats,
            calendar,
            pending,
            error: computed(() => novError.value?.toString() || pubError.value?.toString() || frmError.value?.toString())
        }
    }

    function updateRoute(filter: SearchFilter, merge?: Partial<SearchFilter>) {
        const fil = { ...filter, ...merge };
        const uri = route.path;
        const url = addParams(uri, fil);
        router.push(url);
    }

    function clearFilters() {
        updateRoute({ ...DEFAULT_FILTERS });
    }

    return {
        parse: parseFilters,
        setup,
        update: updateRoute,
        clear: clearFilters
    }
}
