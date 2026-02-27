<template>
    <div class="input-group" :class="classes" :style="actStyles">
        <div class="input-header control fill no-top group center-items flex">
            <input
                class="fill"
                type="date"
                :value="date.toISODate()"
                readonly
                :placeholder="placeholder"
                @focus="toggle"
            />
            <IconBtn icon="content_paste" inline icon-size="16px" @click="fromClipboard" />
            <div class="sep" />
            <slot />
            <IconBtn icon="calendar_month" inline @click="toggle" no-boarder :disabled="isDisabled" />
        </div>

        <div class="popup flex" :class="{ open }">
            <div class="background" @click="toggle" />
            <div class="content flex row center">
                <CalendarSelector
                    v-model="date"
                    :rangeMin="rangeMin"
                    :rangeMax="rangeMax"
                    :theme="theme"
                    @selected="toggle"
                />
            </div>
        </div>
    </div>
</template>

<script lang="ts" setup>
import type { ClassOptions, StyleOptions, DateInput, DateInputTheme, booleanish } from '~/models';
import { DATE_INPUT_THEME } from '~/models';

const dateUtils = useDateUtils();
const { serStyles, serClasses, isTrue } = useUtils();

const props = defineProps<{
    placeholder?: string;
    modelValue: DateInput;
    rangeMin?: DateInput;
    rangeMax?: DateInput;
    'class'?: ClassOptions;
    'styles'?: StyleOptions;
    theme?: Partial<DateInputTheme>;
    disabled?: booleanish;
}>();

const emits = defineEmits<{
    (e: 'update:modelValue', value: Date): void;
}>();

const open = ref(false);
const date = computed({
    get: () => {
        const date = dateUtils.ensureDate(props.modelValue);
        return dateUtils.clamp(date, props.rangeMin, props.rangeMax);
    },
    set: (value: DateInput) => {
        const date = dateUtils.ensureDate(value);
        const clampedDate = dateUtils.clamp(date, props.rangeMin, props.rangeMax);
        emits('update:modelValue', clampedDate.toJSDate());
    }
});

const isDisabled = computed(() => isTrue(props.disabled));

const classes = computed(() => serClasses(props.class));
const actStyles = computed(() => serStyles(props.styles));

const toggle = () => {
    open.value = !open.value;
}

const fromClipboard = async () => {
    const data = await navigator.clipboard.readText();
    if (data) date.value = data;
}

</script>

<style lang="scss" scoped>

.input-group {
    background-color: var(--bg-color-accent);
    border: 1px solid var(--bg-color-accent);
    border-radius: calc(var(--brd-radius) * 2);
    overflow: hidden;

    .input-header {
        background-color: transparent !important;
        border-color: transparent !important;
        margin-top: 0;

        button, a { margin: auto 5px !important; }

        .select-styled {
            background-color: transparent;
            border-color: transparent;
            min-width: 100px;
        }
    }

    &:focus-within {
        border-color: var(--color-primary);
    }

    &.stuck {
        background-color: var(--bg-color-accent-dark);
    }

    .popup {
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        display: none;
        z-index: 1000;

        &.open {
            display: flex;
        }

        .background {
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background-color: rgba(0, 0, 0, 0.5);
        }

        .content {
            background-color: var(--bg-color);
        }
    }
}
</style>
