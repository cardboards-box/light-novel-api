import type { DateTime } from "luxon";

export type booleanish = boolean | 'true' | 'false' | '';
export type booleanishext = booleanish | '';
export type Dictionary<T> = { [key: string]: T };
export type KeyedDictionary<TKey extends string | number | symbol, TValue> = { [key in TKey]: TValue };
export type OKD<TKey extends string | number | symbol, TValue> = { [key in TKey]?: TValue };

export type ClassOptions = string | string[] | undefined | null | {
    [key: string]: booleanishext;
}

export type StyleOptions = string | string[] | undefined | null | {
    [key: string]: string | undefined | null;
}

export type Regions = 'top' | 'left' | 'bottom' | 'right' | 'center';

export interface Rect {
    x: number;
    y: number;
    width: number;
    height: number;
    name: Regions;
}

export const DATE_INPUT_THEME = {
    color: '#c4c0d8',
    colorDisabled: '#5c5870',
    colorSelected: '#61ffca',
    colorBackground: '#282738',
    colorBackgroundDark: '#21202e',
    size: '1.5em',
    margin: '1rem',
    padding: '0.25em',
    fontSize: '1rem'
} as const;

// export const DATE_INPUT_THEME = {
//     color: 'var(--color)',
//     colorDisabled: 'var(--color-muted)',
//     colorSelected: 'var(--color-primary)',
//     colorBackground: 'var(--bg-color)',
//     colorBackgroundDark: 'var(--bg-color-accent-dark)',
//     size: '1.5em',
//     margin: 'var(--margin)',
//     padding: '0.25em',
//     fontSize: '1rem'
// } as const;

export type DateInput = Date | string | DateTime;

export type DateInputTheme = {
    [key in keyof typeof DATE_INPUT_THEME]: string;
};
