interface Action {
    type: string;
}

interface PayloadAction<TPayload> {
    type: string;
    payload: TPayload;
}

interface ErrorAction {
    type: string;
    payload: Error;
    error: boolean;
}
