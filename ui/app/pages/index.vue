<template>
    <CardList
        title="Novel Releases"
        :items="results"
        :pending="pending"
        :error="error"
        v-model="infinite"
        @onscrolled="onScroll"
        @headerstuck="(v) => headerStuck = v"
        @reload="() => refresh()"
        @load-page="(v) => updateRoute({ page: v })"
        capitalize
        allow-reload
        :pagination="{
            page: filter.page ?? 1,
            pages: pages,
            size: filter.size ?? 20,
            total: total
        }"
    >
        <template #default="{ item }">
            <NovelCard :novel="item" />
        </template>
        <template #header>
            <InputGroup
                v-model="searchFilters.search"
                placeholder="Search for your favourite novel!"
                :stuck="headerStuck"
                @search="() => doSearch()"
                is-drawer
            >
                <h3 class="margin-bottom">Advanced Search Options:</h3>

                <label>Formats:</label>
                <ButtonGroup :options="formats" v-model="searchFilters.format" />

                <label>Publishers:</label>
                <ButtonGroup :options="publishers" v-model="searchFilters.publisher" />

                <label class="margin-top">Filter Range Start Date:</label>
                <ButtonGroupBool v-model="startSet" on="Filter" off="Don't Filter" />
                <CalendarInput
                    v-if="searchFilters.start"
                    placeholder="Filter Start Date"
                    v-model="searchFilters.start"
                    :range-max="searchFilters.end"
                />

                <label class="margin-top">Filter Range End Date:</label>
                <ButtonGroupBool v-model="endSet" on="Filter" off="Don't Filter" />
                <CalendarInput
                    v-if="searchFilters.end"
                    placeholder="Filter End Date"
                    v-model="searchFilters.end"
                    :range-min="searchFilters.start"
                />

                <label class="margin-top">Order Direction:</label>
                <ButtonGroupBool v-model="searchFilters.asc" on="Ascending" off="Descending" />

                <div class="flex align-left">
                    <button class="icon-btn" @click="clearFilters()">
                        <Icon>delete</Icon>
                        <p>Clear</p>
                    </button>
                    <button class="icon-btn" @click="doSearch()">
                        <Icon>search</Icon>
                        <p>Search</p>
                    </button>
                </div>
            </InputGroup>
        </template>
    </CardList>
</template>

<script setup lang="ts">
import type { LocationQueryValue } from 'vue-router';
import type { SearchFilter } from '~/models';
import { LncFormat } from '~/models';

const api = useApi();
const route = useRoute();
const { throttle } = useUtils();
const { addParams } = useSettingsHelper();
const router = useRouter();

const DEFAULT_FILTERS: SearchFilter = {
    page: 1,
    size: 20,
    asc: false,
    search: '',
    format: [LncFormat.Digital, LncFormat.Audio],
    isbn: [],
} as const;

const _error = ref<string>();
const headerStuck = ref(false);
const filter = computed(() => parseFilters());
const searchFilters = ref(parseFilters());

const startSet = computed({
    get: () => !!searchFilters.value.start,
    set: (value) => searchFilters.value.start = value ? new Date() : undefined
});
const endSet = computed({
    get: () => !!searchFilters.value.end,
    set: (value) => searchFilters.value.end = value ? new Date() : undefined
});

const page = computed({
    get: () => filter.value.page ?? 1,
    set: (value) => updateRoute({ page: value })
});

const size = computed({
    get: () => filter.value.size ?? 20,
    set: (value) => updateRoute({ size: value })
});

const { pending: novPending, error: novError, data: novels, refresh } = useAsyncData(
    `search-${route.fullPath}`,
    async () => await api.promise.novels.search(parseFilters()),
    { watch: [() => route.query] }
);
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

const results = computed(() => (novels.value ? api.data(novels.value)?.data : []) ?? []);
const total = computed(() => (novels.value ? api.data(novels.value)?.total : 0));
const pages = computed(() => (novels.value ? api.data(novels.value)?.pages : 0));
const pending = computed(() => novPending.value || pubPending.value || frmPending.value);
const error = computed({
    get: () => _error.value || novError.value?.toString() || pubError.value?.toString() || frmError.value?.toString(),
    set: (value) => _error.value = value
});

const _infinite = ref<boolean>();
const infinite = computed<boolean>({
    get: () => _infinite.value ??= localStorage.getItem('infiniteScroll') === 'true',
    set: (value: boolean) => {
        _infinite.value = value;
        localStorage.setItem('infiniteScroll', value.toString());
    }
});

function onScroll() {
    if (results.value.length === 0 ||
        pages.value <= page.value||
        pending.value ||
        !infinite.value)
        return;

    page.value++;
}

function doSearch() {
    updateRoute({
        ...searchFilters.value,
        page: 1
    });
}

function updateRoute(merge?: Partial<SearchFilter>) {
    const fil = { ...filter.value, ...merge };
    const uri = '/';
    const url = addParams(uri, fil);
    router.push(url);
}

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
        'page': { prop: 'page', massage: (v) => +(v ?? 1) },
        'size': { prop: 'size', massage: (v) => +(v ?? 20) },
        'start': { prop: 'start', massage: (v) => new Date(v?.toString() ?? Date.now()) },
        'end': { prop: 'end', massage: (v) => new Date(v?.toString() ?? Date.now()) },
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

function clearFilters() {
    const curPage = page.value;
    const curSize = size.value;
    updateRoute({
        ...DEFAULT_FILTERS,
        page: curPage,
        size: curSize
    });
}

const tRefresh = throttle<void>(() => {
    if (pending.value) return;
    refresh();
}, 250);
watch(() => route.query, () => tRefresh(), { deep: true });
</script>

<style lang="scss" scoped>

</style>
