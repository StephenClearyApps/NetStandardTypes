import React from 'react';

function checkStatus(response: Response): Response {
    if (response.status >= 200 && response.status < 300) {
        return response;
    } else {
        const error = new Error(response.statusText);
        (error as any).response = response;
        throw error;
    }
}

function parseJson(response: Response): Promise<any> {
    return response.json();
}

export function fetchJson<T>(url: string): Promise<T> {
    return window.fetch(url)
        .then(checkStatus)
        .then(parseJson);
}

export function spread<T>(source: T): T {
    return Object.assign({}, source);
}

export function toUInt32(x) {
    return x >>> 0;
}

export function toInt32(x) {
    return x >> 0;
}

export function valueAsInt32IsNegative(x: number): boolean {
    return toUInt32(x) >= Math.pow(2, 31);
}

export function valueAsInt16IsNegative(x: number): boolean {
    return toUInt32(x) >= Math.pow(2, 15);
}

export function toInt16(x: number): number {
    const value = toUInt32(x);
    if (valueAsInt16IsNegative(value)) {
        return value - Math.pow(2, 16);
    } else {
        return value;
    }
}

const hexChars = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'];

export function hex4(x: number): string {
    return '' +
        hexChars[(x >> 12) & 0xF] +
        hexChars[(x >> 8) & 0xF] +
        hexChars[(x >> 4) & 0xF] +
        hexChars[(x >> 0) & 0xF];
}

export function hex8(x: number): string {
    return '' +
        hexChars[(x >> 28) & 0xF] +
        hexChars[(x >> 24) & 0xF] +
        hexChars[(x >> 20) & 0xF] +
        hexChars[(x >> 16) & 0xF] +
        hexChars[(x >> 12) & 0xF] +
        hexChars[(x >> 8) & 0xF] +
        hexChars[(x >> 4) & 0xF] +
        hexChars[(x >> 0) & 0xF];
}

export function preventDefault(e: React.SyntheticEvent) {
    e.preventDefault();
}