type keyBind = {
    [onKeyDown: string]: string;
}

class KeyboardManager { // this is a SINGLETON class! becasue i don't want to useContext for stuff that don't involve jsx components
    private static instance: KeyboardManager;
    private keyState: Set<string> = new Set(); // keys currently being pressed
    private keyBinds: keyBind = {}; // the set of keybinds
    private subscribers: Set<() => void> = new Set(); // the set of "subscribed" callback functions

    private constructor() {
        this.handleKeyDown = this.handleKeyDown.bind(this);
        this.handleKeyUp = this.handleKeyUp.bind(this);
        window.addEventListener("keydown", this.handleKeyDown);
        window.addEventListener("keyup", this.handleKeyUp);
    }

    public static getInstance(): KeyboardManager {
        if (!KeyboardManager.instance) {
            KeyboardManager.instance = new KeyboardManager();
        }
        return KeyboardManager.instance;
    }

    public loadKeybinds(bindings: keyBind) {
        this.keyBinds = bindings;
    }

    private handleKeyDown(event: KeyboardEvent) {
        this.keyState.add(event.code);
        this.notify();
    }

    private handleKeyUp(event: KeyboardEvent) {
        this.keyState.delete(event.code);
        this.notify();
    }

    public isActionPressed(action: string): boolean {
        const key = this.keyBinds[action];
        return this.keyState.has(key);
    }

    public subscribe(callback: () => void) {
        this.subscribers.add(callback);
    }

    public unsubscribe(callback: () => void) {
        this.subscribers.delete(callback);
    }

    private notify() {
        for (const callback of this.subscribers) callback();
    }
}

export default KeyboardManager