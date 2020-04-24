# Understanding (all) JavaScript module formats and tools

When you build an application with JavaScript, you always want to modularize your code. However, JavaScript language was initially invented for simple form manipulation, with no built-in features like module or namespace. In years, tons of technologies are invented to modularize JavaScript. This article discusses all mainstream terms, patterns, libraries, syntax, and tools for JavaScript modules.

- [Understanding (all) JavaScript module formats and tools](#understanding-all-javascript-module-formats-and-tools)
  - [IIFE module: JavaScript module pattern](#iife-module-javascript-module-pattern)
    - [IIFE: Immediately invoked function expression](#iife-immediately-invoked-function-expression)
    - [Import mixins](#import-mixins)
  - [Revealing module: JavaScript revealing module pattern](#revealing-module-javascript-revealing-module-pattern)
  - [CJS module: CommonJS module, or Node.js module](#cjs-module-commonjs-module-or-nodejs-module)
  - [AMD module: Asynchronous Module Definition, or RequireJS module](#amd-module-asynchronous-module-definition-or-requirejs-module)
    - [Dynamic loading](#dynamic-loading)
    - [AMD module from CommonJS module](#amd-module-from-commonjs-module)
  - [UMD module: Universal Module Definition, or UmdJS module](#umd-module-universal-module-definition-or-umdjs-module)
    - [UMD for both AMD (RequireJS) and native browser](#umd-for-both-amd-requirejs-and-native-browser)
    - [UMD for both AMD (RequireJS) and CommonJS (Node.js)](#umd-for-both-amd-requirejs-and-commonjs-nodejs)
  - [ES module: ECMAScript 2015, or ES6 module](#es-module-ecmascript-2015-or-es6-module)
  - [ES dynamic module: ECMAScript 2020, or ES11 dynamic module](#es-dynamic-module-ecmascript-2020-or-es11-dynamic-module)
  - [System module: SystemJS module](#system-module-systemjs-module)
    - [Dynamic module loading](#dynamic-module-loading)
  - [Webpack module: bundle from CJS, AMD, ES modules](#webpack-module-bundle-from-cjs-amd-es-modules)
  - [Babel module: transpile from ES module](#babel-module-transpile-from-es-module)
    - [Babel with SystemJS](#babel-with-systemjs)
  - [TypeScript module: Transpile to CJS, AMD, ES, System modules](#typescript-module-transpile-to-cjs-amd-es-system-modules)
    - [Internal module and namespace](#internal-module-and-namespace)
  - [Conclusion](#conclusion)

## IIFE module: JavaScript module pattern

In the browser, defining a JavaScript variable is defining a global variable, which causes pollution across all JavaScript files loaded by the current web page:

```js
// Define global variables.
let count = 0;
const increase = () => ++count;
const reset = () => {
    count = 0;
    console.log("Count is reset.");
};

// Use global variables.
increase();
reset();
```

To avoid global pollution, an anonymous function can be used to wrap the code:

```js
(() => {
    let count = 0;
    // ...
});
```

Apparently, there is no longer any global variable. However, defining a function does not execute the code inside the function.

### IIFE: Immediately invoked function expression

To execute the code inside a function `f`, the syntax is function call `()` as `f()`. To execute the code inside an anonymous function `(() => {})`, the same function call syntax `()` can be used as `(() => {})()`:

```js
(() => {
    let count = 0;
    // ...
})();
```

This is called an IIFE (Immediately invoked function expression). So a basic module can be defined in this way:

```js
// Define IIFE module.
const iifeCounterModule = (() => {
    let count = 0;
    return {
        increase: () => ++count,
        reset: () => {
            count = 0;
            console.log("Count is reset.");
        }
    };
})();

// Use IIFE module.
iifeCounterModule.increase();
iifeCounterModule.reset();
```

It wraps the module code inside an IIFE. The anonymous function returns an object, which is the placeholder of exported APIs. Only 1 global variable is introduced, which is the module name (or namespace). Later the module name can be used to call the exported module APIs. This is called the module pattern of JavaScript.

### Import mixins

When defining a module, some dependencies may be required. With IIFE module pattern, each dependent module is a global variable. The dependent modules can be directly accessed inside the anonymous function, or they can be passed as the anonymous function’s arguments:

```js
// Define IIFE module with dependencies.
const iifeCounterModule = ((dependencyModule1, dependencyModule2) => {
    let count = 0;
    return {
        increase: () => ++count,
        reset: () => {
            count = 0;
            console.log("Count is reset.");
        }
    };
})(dependencyModule1, dependencyModule2);
```

The early version of popular libraries, like jQuery, followed this pattern. (The latest version of jQuery follows the UMD module, which is explained later in this article.)

## Revealing module: JavaScript revealing module pattern

The revealing module pattern is named by Christian Heilmann. This pattern is also an IIFE, but it emphasizes defining all APIs as local variables inside the anonymous function:

```js
// Define revealing module.
const revealingCounterModule = (() => {
    let count = 0;
    const increase = () => ++count;
    const reset = () => {
        count = 0;
        console.log("Count is reset.");
    };

    return {
        increase,
        reset
    };
})();

// Use revealing module.
revealingCounterModule.increase();
revealingCounterModule.reset();
```

With this syntax, it becomes easier when the APIs need to call each other.

## CJS module: CommonJS module, or Node.js module

CommonJS, initially named ServerJS, is a pattern to define and consume modules. It is implemented by Node,js. By default, each `.js` file is a CommonJS module. A `module` variable and an `exports` variable are provided for a module (a file) to expose APIs. And a `require` function is provided to load and consume a module. The following code defines the counter module in CommonJS syntax:

```js
// Define CommonJS module: commonJSCounterModule.js.
const dependencyModule1 = require("./dependencyModule1");
const dependencyModule2 = require("./dependencyModule2");

let count = 0;
const increase = () => ++count;
const reset = () => {
    count = 0;
    console.log("Count is reset.");
};

exports.increase = increase;
exports.reset = reset;
// Or equivalently:
module.exports = {
    increase,
    reset
};
```

The following example consumes the counter module:

```js
// Use CommonJS module.
const { increase, reset } = require("./commonJSCounterModule");
increase();
reset();
// Or equivelently:
const commonJSCounterModule = require("./commonJSCounterModule");
commonJSCounterModule.increase();
commonJSCounterModule.reset();
```

At runtime, Node.js implements this by wrapping the code inside the file into a function, then passes the `exports` variable, `module` variable, and `require` function through arguments.

```js
// Define CommonJS module: wrapped commonJSCounterModule.js.
(function (exports, require, module, __filename, __dirname) {
    const dependencyModule1 = require("./dependencyModule1");
    const dependencyModule2 = require("./dependencyModule2");

    let count = 0;
    const increase = () => ++count;
    const reset = () => {
        count = 0;
        console.log("Count is reset.");
    };

    module.exports = {
        increase,
        reset
    };

    return module.exports;
}).call(thisValue, exports, require, module, filename, dirname);

// Use CommonJS module.
(function (exports, require, module, __filename, __dirname) {
    const commonJSCounterModule = require("./commonJSCounterModule");
    commonJSCounterModule.increase();
    commonJSCounterModule.reset();
}).call(thisValue, exports, require, module, filename, dirname);
```

## AMD module: Asynchronous Module Definition, or RequireJS module

AMD (Asynchronous Module Definition <https://github.com/amdjs/amdjs-api>), is a pattern to define and consume module. It is implemented by RequireJS library <https://requirejs.org/>. AMD provides a `define` function to define module, which accepts the module name, dependent modules’ names, and a factory function:

```js
// Define AMD module.
define("amdCounterModule", ["dependencyModule1", "dependencyModule2"], (dependencyModule1, dependencyModule2) => {
    let count = 0;
    const increase = () => ++count;
    const reset = () => {
        count = 0;
        console.log("Count is reset.");
    };

    return {
        increase,
        reset
    };
});
```

It also provides a `require` function to consume module:

```js
// Use AMD module.
require(["amdCounterModule"], amdCounterModule => {
    amdCounterModule.increase();
    amdCounterModule.reset();
});
```

The AMD `require` function is totally different from the CommonJS `require` function. AMD `require` accept the names of modules to be consumed, and pass the module to a function argument.

### Dynamic loading

AMD’s `define` function has another overload. It accepts a callback function, and pass a CommonJS-like `require` function to that callback. Inside the callback function, `require` can be called to dynamically load the module:

```js
// Use dynamic AMD module.
define(require => {
    const dynamicDependencyModule1 = require("dependencyModule1");
    const dynamicDependencyModule2 = require("dependencyModule2");

    let count = 0;
    const increase = () => ++count;
    const reset = () => {
        count = 0;
        console.log("Count is reset.");
    };

    return {
        increase,
        reset
    };
});
```

### AMD module from CommonJS module

The above `define` function overload can also passes the `require` function as well as `exports` variable and `module` to its callback function. So inside the callback, CommonJS syntax code can work:

```js
// Define AMD module with CommonJS code.
define((require, exports, module) => {
    // CommonJS code.
    const dependencyModule1 = require("dependencyModule1");
    const dependencyModule2 = require("dependencyModule2");

    let count = 0;
    const increase = () => ++count;
    const reset = () => {
        count = 0;
        console.log("Count is reset.");
    };

    exports.increase = increase;
    exports.reset = reset;
});

// Use AMD module with CommonJS code.
define(require => {
    // CommonJS code.
    const counterModule = require("amdCounterModule");
    counterModule.increase();
    counterModule.reset();
});
```

## UMD module: Universal Module Definition, or UmdJS module

UMD (Universal Module Definition, <https://github.com/umdjs/umd>) is a set of tricky patterns to make your code file work in multiple environments.

### UMD for both AMD (RequireJS) and native browser

For example, the following is a kind of UMD pattern to make module definition work with both AMD (RequireJS) and native browser:

```js
// Define UMD module for both AMD and browser.
((root, factory) => {
    // Detects AMD/RequireJS"s define function.
    if (typeof define === "function" && define.amd) {
        // Is AMD/RequireJS. Call factory with AMD/RequireJS"s define function.
        define("umdCounterModule", ["deependencyModule1", "dependencyModule2"], factory);
    } else {
        // Is Browser. Directly call factory.
        // Imported dependencies are global variables(properties of window object).
        // Exported module is also a global variable(property of window object)
        root.umdCounterModule = factory(root.deependencyModule1, root.dependencyModule2);
    }
})(typeof self !== "undefined" ? self : this, (deependencyModule1, dependencyModule2) => {
    // Module code goes here.
    let count = 0;
    const increase = () => ++count;
    const reset = () => {
        count = 0;
        console.log("Count is reset.");
    };

    return {
        increase,
        reset
    };
});
```

It is more complex but it is just an IIFE. The anonymous function detects if AMD’s `define` function exists.

- If yes, call the module factory with AMD’s `define` function.
- If not, it calls the module factory directly. At this moment,  the `root` argument is actually the browser’s `window` object. It gets dependency modules from global variables (properties of `window` object). When `factory` returns the module, the returned module is also assigned to a global variable (property of `window` object).

### UMD for both AMD (RequireJS) and CommonJS (Node.js)

The following is another kind of UMD pattern to make module definition work with both AMD (RequireJS) and CommonJS (Node.js):

```js
(define => define((require, exports, module) => {
    // Module code goes here.
    const dependencyModule1 = require("dependencyModule1");
    const dependencyModule2 = require("dependencyModule2");

    let count = 0;
    const increase = () => ++count;
    const reset = () => {
        count = 0;
        console.log("Count is reset.");
    };

    module.export = {
        increase,
        reset
    };
}))(// Detects module variable and exports variable of CommonJS/Node.js.
    // Also detect the define function of AMD/RequireJS.
    typeof module === "object" && module.exports && typeof define !== "function"
        ? // Is CommonJS/Node.js. Manually create a define function.
            factory => module.exports = factory(require, exports, module)
        : // Is AMD/RequireJS. Directly use its define function.
            define);
```

Again, don’t be scared. It is just another IIFE. When the anonymous function is called, its argument is evaluated. The argument evaluation detects the environment (check the module variable and `exports` variable of CommonJS/Node.js, as well as the `define` function of AMD/RequireJS).

- If the environment is CommonJS/Node.js, the anonymous function’s argument is a manually created `define` function.
- If the environment is AMD/RequireJS, the anonymous function’s argument is just AMD’s `define` function. So when the anonymous function is executed, it is guaranteed to have a working `define` function. Inside the anonymous function, it simply calls the `define` function to create the module.

## ES module: ECMAScript 2015, or ES6 module

After all the module mess, in 2015, JavaScript’s spec version 6 introduces one more different module syntax. This spec is called ECMAScript 2015 (ES2015), or ECMAScript 6 (ES6). The main syntax is the import keyword and the export keyword. The following example uses new syntax to demonstrate ES module’s named import/export and default import/export:

```js
// Define ES module: esCounterModule.js or esCounterModule.mjs.
import dependencyModule1 from "./dependencyModule1.mjs";
import dependencyModule2 from "./dependencyModule2.mjs";

let count = 0;
// Named export:
export const increase = () => ++count;
export const reset = () => {
    count = 0;
    console.log("Count is reset.");
};
// Or default export:
export default {
    increase,
    reset
};
```

To use this module file in browser, add a `<script>` tag and specify it is a module: `<script type="module" src="esCounterModule.js"></script>`. To use this module file in Node.js, rename its extension from `.js` to `.mjs`.

```js
// Use ES module.
// Browser: <script type="module" src="esCounterModule.js"></script> or inline.
// Server: esCounterModule.mjs
// Import from named export.
import { increase, reset } from "./esCounterModule.mjs";
increase();
reset();
// Or import from default export:
import esCounterModule from "./esCounterModule.mjs";
esCounterModule.increase();
esCounterModule.reset();
```

For browser, `<script>`’s `nomodule` attribute can be used for fallback:

```html
<script nomodule>
    alert("Not supported.");
</script>
```

## ES dynamic module: ECMAScript 2020, or ES11 dynamic module

In 2020, the latest JavaScript spec version 11 is introducing a built-in function `import` to consume an ES module dynamically. The `import` function returns a `promise`, so its `then` method can be called to consume the module:

```js
// Use dynamic ES module with promise APIs, import from named export:
import("./esCounterModule.js").then(({ increase, reset }) => {
    increase();
    reset();
});
// Or import from default export:
import("./esCounterModule.js").then(dynamicESCounterModule => {
    dynamicESCounterModule.increase();
    dynamicESCounterModule.reset();
});
```

By returning a `promise`, apparently, `import` function can also work with the `await` keyword:

```js
// Use dynamic ES module with async/await.
(async () => {

    // Import from named export:
    const { increase, reset } = await import("./esCounterModule.js");
    increase();
    reset();

    // Or import from default export:
    const dynamicESCounterModule = await import("./esCounterModule.js");
    dynamicESCounterModule.increase();
    dynamicESCounterModule.reset();

})();
```

The following is the compatibility of import/dynamic import/export, from <https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Modules>:

![import compatibility](https://aspblogs.blob.core.windows.net/media/dixin/Open-Live-Writer/JavaScript-moduels_11325/image_2.png)

![export compatibility](https://aspblogs.blob.core.windows.net/media/dixin/Open-Live-Writer/JavaScript-moduels_11325/image_4.png)

## System module: SystemJS module

SystemJS is a library that can enable ES module syntax for older ES. For example, the following module is defined in ES 6syntax:

```js
// Define ES module.
import dependencyModule1 from "./dependencyModule1.js";
import dependencyModule2 from "./dependencyModule2.js";
dependencyModule1.api1();
dependencyModule2.api2();

let count = 0;
// Named export:
export const increase = function () { return ++count };
export const reset = function () {
    count = 0;
    console.log("Count is reset.");
};
// Or default export:
export default {
    increase,
    reset
}
```

If the current runtime, like an old browser, does not support ES6 syntax, the above code cannot work. One solution is to transpile the above module definition to a call of SystemJS library API, `System.register`:

```js
// Define SystemJS module.
System.register(["./dependencyModule1.js", "./dependencyModule2.js"], function (exports_1, context_1) {
    "use strict";
    var dependencyModule1_js_1, dependencyModule2_js_1, count, increase, reset;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (dependencyModule1_js_1_1) {
                dependencyModule1_js_1 = dependencyModule1_js_1_1;
            },
            function (dependencyModule2_js_1_1) {
                dependencyModule2_js_1 = dependencyModule2_js_1_1;
            }
        ],
        execute: function () {
            dependencyModule1_js_1.default.api1();
            dependencyModule2_js_1.default.api2();
            count = 0;
            // Named export:
            exports_1("increase", increase = function () { return ++count };
            exports_1("reset", reset = function () {
                count = 0;
                console.log("Count is reset.");
            };);
            // Or default export:
            exports_1("default", {
                increase,
                reset
            });
        }
    };
});
```

So that the import/export new ES6 syntax is gone. The old API call syntax works for sure. This transpilation can be done automatically with Webpack, TypeScript, etc., which are explained later in this article.

### Dynamic module loading

SystemJS also provides an import function for dynamic import:

```js
// Use SystemJS module with promise APIs.
System.import("./esCounterModule.js").then(dynamicESCounterModule => {
    dynamicESCounterModule.increase();
    dynamicESCounterModule.reset();
});
```

## Webpack module: bundle from CJS, AMD, ES modules

Webpack is a bundler for modules. It transpiles combined CommonJS module, AMD module, and ES module into a single harmony module pattern, and bundle all code into a single file. For example, the following 3 files define 3 modules in 3 different syntaxes:

```js
// Define AMD module: amdDependencyModule1.js
define("amdDependencyModule1", () => {
    const api1 = () => { };
    return {
        api1
    };
});

// Define CommonJS module: commonJSDependencyModule2.js
const dependencyModule1 = require("./amdDependencyModule1");
const api2 = () => dependencyModule1.api1();
exports.api2 = api2;

// Define ES module: esCounterModule.js.
import dependencyModule1 from "./amdDependencyModule1";
import dependencyModule2 from "./commonJSDependencyModule2";
dependencyModule1.api1();
dependencyModule2.api2();

let count = 0;
const increase = () => ++count;
const reset = () => {
    count = 0;
    console.log("Count is reset.");
};

export default {
    increase,
    reset
}
```

And the following file consumes the counter module:

```js
// Use ES module: index.js
import counterModule from "./esCounterModule";
counterModule.increase();
counterModule.reset();
```

Webpack can bundle all the above file, even they are in 3 different module systems, into a single file `main.js`:

- root
  - dist
    - main.js (Bundle of all files under src)
  - src
    - amdDependencyModule1.js
    - commonJSDependencyModule2.js
    - esCounterModule.js
    - index.js
  - webpack.config.js

Since Webpack is based on Node.js, Webpack uses CommonJS module syntax for itself. In `webpack.config.js`:

```js
const path = require('path');

module.exports = {
    entry: './src/index.js',
    mode: "none", // Do not optimize or minimize the code for readability.
    output: {
        filename: 'main.js',
        path: path.resolve(__dirname, 'dist'),
    },
};
```

Now run the following command to transpile and bundle all 4 files, which are in different syntax:

```cmd
npm install webpack webpack-cli --save-dev
npx webpack --config webpack.config.js
```

AS a result, Webpack generates the bundle file `main.js`. The following code in `main.js` is reformatted, and variables are renamed, to improve readability:

```js
(function (modules) { // webpackBootstrap
    // The module cache
    var installedModules = {};
    // The require function
    function require(moduleId) {
        // Check if module is in cache
        if (installedModules[moduleId]) {
            return installedModules[moduleId].exports;

        }
        // Create a new module (and put it into the cache)
        var module = installedModules[moduleId] = {
            i: moduleId,
            l: false,
            exports: {}

        };
        // Execute the module function
        modules[moduleId].call(module.exports, module, module.exports, require);
        // Flag the module as loaded
        module.l = true;
        // Return the exports of the module
        return module.exports;
    }

    // expose the modules object (__webpack_modules__)
    require.m = modules;
    // expose the module cache
    require.c = installedModules;
    // define getter function for harmony exports
    require.d = function (exports, name, getter) {
        if (!require.o(exports, name)) {
            Object.defineProperty(exports, name, { enumerable: true, get: getter });

        }

    };
    // define __esModule on exports
    require.r = function (exports) {
        if (typeof Symbol !== 'undefined' && Symbol.toStringTag) {
            Object.defineProperty(exports, Symbol.toStringTag, { value: 'Module' });

        }
        Object.defineProperty(exports, '__esModule', { value: true });

    };
    // create a fake namespace object
    // mode & 1: value is a module id, require it
    // mode & 2: merge all properties of value into the ns
    // mode & 4: return value when already ns object
    // mode & 8|1: behave like require
    require.t = function (value, mode) {
        if (mode & 1) value = require(value);
        if (mode & 8) return value;
        if ((mode & 4) && typeof value === 'object' && value && value.__esModule) return value;
        var ns = Object.create(null);
        require.r(ns);
        Object.defineProperty(ns, 'default', { enumerable: true, value: value });
        if (mode & 2 && typeof value != 'string') for (var key in value) require.d(ns, key, function (key) { return value[key]; }.bind(null, key));
        return ns;
    };
    // getDefaultExport function for compatibility with non-harmony modules
    require.n = function (module) {
        var getter = module && module.__esModule ?
            function getDefault() { return module['default']; } :
            function getModuleExports() { return module; };
        require.d(getter, 'a', getter);
        return getter;
    };
    // Object.prototype.hasOwnProperty.call
    require.o = function (object, property) { return Object.prototype.hasOwnProperty.call(object, property); };
    // __webpack_public_path__
    require.p = "";
    // Load entry module and return exports
    return require(require.s = 0);
})([
    function (module, exports, require) {
        "use strict";
        require.r(exports);
        // Use ES module: index.js.
        var esCounterModule = require(1);
        esCounterModule["default"].increase();
        esCounterModule["default"].reset();
    },
    function (module, exports, require) {
        "use strict";
        require.r(exports);
        // Define ES module: esCounterModule.js.
        var amdDependencyModule1 = require.n(require(2));
        var commonJSDependencyModule2 = require.n(require(3));
        amdDependencyModule1.a.api1();
        commonJSDependencyModule2.a.api2();

        let count = 0;
        const increase = () => ++count;
        const reset = () => {
            count = 0;
            console.log("Count is reset.");
        };

        exports["default"] = {
            increase,
            reset
        };
    },
    function (module, exports, require) {
        var result;
        !(result = (() => {
            // Define AMD module: amdDependencyModule1.js
            const api1 = () => { };
            return {
                api1
            };
        }).call(exports, require, exports, module),
            result !== undefined && (module.exports = result));
    },
    function (module, exports, require) {
        // Define CommonJS module: commonJSDependencyModule2.js
        const dependencyModule1 = require(2);
        const api2 = () => dependencyModule1.api1();
        exports.api2 = api2;
    }
]);
```

Again, it is just another IIFE. The code of all 4 files is transpiled to the code in 4 functions in an array. And that array is passed to the anonymous function as an argument.

## Babel module: transpile from ES module

Babel is another transpiler to convert ES6+ JavaScript code to the older syntax for the older environment like older browsers. The above counter module in ES6 import/export syntax can be converted to the following babel module with new syntax replaced:

```js
// Babel.
Object.defineProperty(exports, "__esModule", {
    value: true
});
exports["default"] = void 0;
function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

// Define ES module: esCounterModule.js.
var dependencyModule1 = _interopRequireDefault(require("./amdDependencyModule1"));
var dependencyModule2 = _interopRequireDefault(require("./commonJSDependencyModule2"));
dependencyModule1["default"].api1();
dependencyModule2["default"].api2();

var count = 0;
var increase = function () { return ++count; };
var reset = function () {
    count = 0;
    console.log("Count is reset.");
};

exports["default"] = {
    increase: increase,
    reset: reset
};
```

And here is the code in `index.js` which consumes the counter module:

```js
// Babel.
function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

// Use ES module: index.js
var esCounterModule = _interopRequireDefault(require("./esCounterModule.js"));
esCounterModule["default"].increase();
esCounterModule["default"].reset();
```

This is the default transpilation. Babel can also work with other tools.

### Babel with SystemJS

SystemJS can be used as a plugin for Babel:

```cmd
npm install --save-dev @babel/plugin-transform-modules-systemjs
```

And it should be added to the Babel configuration `babel.config.json`:

```js
{
    "plugins": ["@babel/plugin-transform-modules-systemjs"],
    "presets": [
        [
            "@babel/env",
            {
                "targets": {
                    "ie": "11"
                }
            }
        ]
    ]
}
```

Now Babel can work with SystemJS to transpile CommonJS/Node.js module, AMD/RequireJS module, and ES module:

```cmd
npx babel src --out-dir lib
```

The result is:

- root
  - lib
    - amdDependencyModule1.js (Transpiled with SystemJS)
    - commonJSDependencyModule2.js (Transpiled with SystemJS)
    - esCounterModule.js (Transpiled with SystemJS)
    - index.js (Transpiled with SystemJS)
  - src
    - amdDependencyModule1.js
    - commonJSDependencyModule2.js
    - esCounterModule.js
    - index.js
  - babel.config.json

Now all the ADM, CommonJS, and ES module syntax are transpiled to SystemJS syntax:

```js
// Transpile AMD/RequireJS module definition to SystemJS syntax: lib/amdDependencyModule1.js.
System.register([], function (_export, _context) {
    "use strict";
    return {
        setters: [],
        execute: function () {
            // Define AMD module: src/amdDependencyModule1.js
            define("amdDependencyModule1", () => {
                const api1 = () => { };

                return {
                    api1
                };
            });
        }
    };
});

// Transpile CommonJS/Node.js module definition to SystemJS syntax: lib/commonJSDependencyModule2.js.
System.register([], function (_export, _context) {
    "use strict";
    var dependencyModule1, api2;
    return {
        setters: [],
        execute: function () {
            // Define CommonJS module: src/commonJSDependencyModule2.js
            dependencyModule1 = require("./amdDependencyModule1");

            api2 = () => dependencyModule1.api1();

            exports.api2 = api2;
        }
    };
});

// Transpile ES module definition to SystemJS syntax: lib/esCounterModule.js.
System.register(["./amdDependencyModule1", "./commonJSDependencyModule2"], function (_export, _context) {
    "use strict";
    var dependencyModule1, dependencyModule2, count, increase, reset;
    return {
        setters: [function (_amdDependencyModule) {
            dependencyModule1 = _amdDependencyModule.default;
        }, function (_commonJSDependencyModule) {
            dependencyModule2 = _commonJSDependencyModule.default;
        }],
        execute: function () {
            // Define ES module: src/esCounterModule.js.
            dependencyModule1.api1();
            dependencyModule2.api2();
            count = 0;

            increase = () => ++count;

            reset = () => {
                count = 0;
                console.log("Count is reset.");
            };

            _export("default", {
                increase,
                reset
            });
        }
    };
});

// Transpile ES module usage to SystemJS syntax: lib/index.js.
System.register(["./esCounterModule"], function (_export, _context) {
    "use strict";
    var esCounterModule;
    return {
        setters: [function (_esCounterModuleJs) {
            esCounterModule = _esCounterModuleJs.default;
        }],
        execute: function () {
            // Use ES module: src/index.js
            esCounterModule.increase();
            esCounterModule.reset();
        }
    };
});
```

## TypeScript module: Transpile to CJS, AMD, ES, System modules

TypeScript supports all JavaScript syntax, including the ES6 module syntax <https://www.typescriptlang.org/docs/handbook/modules.html>. When TypeScript transpiles, the ES module code can either be kept as ES6, or transpiled to other formats, including CommonJS/Node.js, AMD/RequireJS, UMD/UmdJS, or System/SystemJS, according to the specified transpiler options in `tsconfig.json`:

```js
{
    "compilerOptions": {
        "module": "ES2020", // None, CommonJS, AMD, System, UMD, ES6, ES2015, ES2020, ESNext.
    }
}
```

For example:

```js
// TypeScript and ES module.
// With compilerOptions: { module: "ES6" }. Transpile to ES module with the same import/export syntax.
import dependencyModule from "./dependencyModule";
dependencyModule.api();
let count = 0;
export const increase = function () { return ++count };


// With compilerOptions: { module: "CommonJS" }. Transpile to CommonJS/Node.js module:
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
exports.__esModule = true;

var dependencyModule_1 = __importDefault(require("./dependencyModule"));
dependencyModule_1["default"].api();
var count = 0;
exports.increase = function () { return ++count; };

// With compilerOptions: { module: "AMD" }. Transpile to AMD/RequireJS module:
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
define(["require", "exports", "./dependencyModule"], function (require, exports, dependencyModule_1) {
    "use strict";
    exports.__esModule = true;

    dependencyModule_1 = __importDefault(dependencyModule_1);
    dependencyModule_1["default"].api();
    var count = 0;
    exports.increase = function () { return ++count; };
});

// With compilerOptions: { module: "UMD" }. Transpile to UMD/UmdJS module:
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
(function (factory) {
    if (typeof module === "object" && typeof module.exports === "object") {
        var v = factory(require, exports);
        if (v !== undefined) module.exports = v;
    }
    else if (typeof define === "function" && define.amd) {
        define(["require", "exports", "./dependencyModule"], factory);
    }
})(function (require, exports) {
    "use strict";
    exports.__esModule = true;

    var dependencyModule_1 = __importDefault(require("./dependencyModule"));
    dependencyModule_1["default"].api();
    var count = 0;
    exports.increase = function () { return ++count; };
});

// With compilerOptions: { module: "System" }. Transpile to System/SystemJS module:
System.register(["./dependencyModule"], function (exports_1, context_1) {
    "use strict";
    var dependencyModule_1, count, increase;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (dependencyModule_1_1) {
                dependencyModule_1 = dependencyModule_1_1;
            }
        ],
        execute: function () {
            dependencyModule_1["default"].api();
            count = 0;
            exports_1("increase", increase = function () { return ++count; });
        }
    };
});
```

The ES module syntax supported in TypeScript was called external modules.

### Internal module and namespace

TypeScript also has a `module` keyword and a `namespace` keyword <https://www.typescriptlang.org/docs/handbook/namespaces-and-modules.html#pitfalls-of-namespaces-and-modules>. They were called internal modules:

```ts
module Counter {
    let count = 0;
    export const increase = () => ++count;
    export const reset = () => {
        count = 0;
        console.log("Count is reset.");
    };
}

namespace Counter {
    let count = 0;
    export const increase = () => ++count;
    export const reset = () => {
        count = 0;
        console.log("Count is reset.");
    };
}
```

They are both transpiled to JavaScript objects:

```js
var Counter;
(function (Counter) {
    var count = 0;
    Counter.increase = function () { return ++count; };
    Counter.reset = function () {
        count = 0;
        console.log("Count is reset.");
    };
})(Counter || (Counter = {}));
```

TypeScript module and namespace can have multiple levels by supporting the `.` separator:

```ts
module Counter.Sub {
    let count = 0;
    export const increase = () => ++count;
}

namespace Counter.Sub {
    let count = 0;
    export const increase = () => ++count;
}
```

The the sub module and sub namespace are both transpiled to object’s property:

```js
var Counter;
(function (Counter) {
    var Sub;
    (function (Sub) {
        var count = 0;
        Sub.increase = function () { return ++count; };
    })(Sub = Counter.Sub || (Counter.Sub = {}));
})(Counter|| (Counter = {}));
```

TypeScript module and namespace can also be used in the `export` statement:

```ts
module Counter {
    let count = 0;
    export module Sub {
        export const increase = () => ++count;
    }
}

module Counter {
    let count = 0;
    export namespace Sub {
        export const increase = () => ++count;
    }
}
```

The transpilation is the same as submodule and sub-namespace:

```js
var Counter;
(function (Counter) {
    var count = 0;
    var Sub;
    (function (Sub) {
        Sub.increase = function () { return ++count; };
    })(Sub = Counter.Sub || (Counter.Sub = {}));
})(Counter || (Counter = {}));
```

## Conclusion

Welcome to JavaScript, which has so much drama - 10+ systems/formats just for modularization/namespace:

1. IIFE module: JavaScript **module pattern**
1. Revealing module: JavaScript **revealing module pattern**
1. **CJS module**: CommonJS module, or Node.js module
1. **AMD module**: Asynchronous Module Definition, or RequireJS module
1. **UMD module**: Universal Module Definition, or UmdJS module
1. **ES module**: ECMAScript 2015, or ES6 module
1. **ES dynamic module**: ECMAScript 2020, or ES11 dynamic module
1. **System module**: SystemJS module
1. **Webpack module**: transpile and bundle of CJS, AMD, ES modules
1. **Babel module**: transpile ES module
1. **TypeScript module** and namespace

Fortunately, now JavaScript has standard built-in language features for modules, and it is supported by Node.js and all the latest modern browsers. For the older environments, you can still code with the new ES module syntax, then use Webpack/Babel/SystemJS/TypeScript to transpile to older or compatible syntax.
