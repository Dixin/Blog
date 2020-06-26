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

(() => {
    let count = 0;
    // ...
});

(() => {
    let count = 0;
    // ...
})();

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

// Use CommonJS module.
const commonJSCounterModule = require("./commonJSCounterModule");
commonJSCounterModule.increase();
commonJSCounterModule.reset();
// Or equivelently:
const { increase, reset } = require("./commonJSCounterModule");
increase();
reset();

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

// Use AMD module.
require(["amdCounterModule"], amdCounterModule => {
    amdCounterModule.increase();
    amdCounterModule.reset();
});

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

// Define AMD module using CommonJS code.
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

// Consume AMD module using CommonJS code.
define(require => {
    // CommonJS code.
    const counterModule = require("amdCounterModule");
    counterModule.increase();
    counterModule.reset();
});

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

// Use SystemJS module with promise APIs.
System.import("./esCounterModule.js").then(dynamicESCounterModule => {
    dynamicESCounterModule.increase();
    dynamicESCounterModule.reset();
});

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

// Use ES module: index.js
import counterModule from "./esCounterModule.js";
counterModule.increase();
counterModule.reset();

// Babel.
Object.defineProperty(exports, "__esModule", {
    value: true
});
exports["default"] = void 0;
function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

// Define ES module: esCounterModule.js.
var dependencyModule1 = _interopRequireDefault(require("./dependencyModule1"));
var dependencyModule2 = _interopRequireDefault(require("./dependencyModule2"));
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

// Babel.
function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { "default": obj }; }

// Use ES module: index.js
var esCounterModule = _interopRequireDefault(require("./esCounterModule.js"));
esCounterModule["default"].increase();
esCounterModule["default"].reset();

const babel = {
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
};

// Babel with SystemJS: lib/amdDependencyModule1.js.
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

// Babel with SystemJS: lib/commonJSDependencyModule2.js.
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

// Babel with SystemJS: lib/esCounterModule.js.
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

// Babel with SystemJS: lib/index.js.
System.register(["./esCounterModule.js"], function (_export, _context) {
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

var Counter;
(function (Counter) {
    var count = 0;
    Counter.increase = function () { return ++count; };
    Counter.reset = function () {
        count = 0;
        console.log("Count is reset.");
    };
})(Counter || (Counter = {}));

module Counter.Sub {
    let count = 0;
    export const increase = () => ++count;
}

namespace Counter.Sub {
    let count = 0;
    export const increase = () => ++count;
}

var Counter;
(function (Counter) {
    var Sub;
    (function (Sub) {
        var count = 0;
        Sub.increase = function () { return ++count; };
    })(Sub = Counter.Sub || (Counter.Sub = {}));
})(Counter || (Counter = {}));

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

var Counter;
(function (Counter) {
    var count = 0;
    var Sub;
    (function (Sub) {
        Sub.increase = function () { return ++count; };
    })(Sub = Counter.Sub || (Counter.Sub = {}));
})(Counter || (Counter = {}));

// TypeScript.
import dependencyModule from "./dependencyModule";
dependencyModule.api();
let count = 0;
export const increase = function () { return ++count };

// Transpile to CommonJS/Node.js:
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
exports.__esModule = true;

var dependencyModule_1 = __importDefault(require("./dependencyModule"));
dependencyModule_1["default"].api();
var count = 0;
exports.increase = function () { return ++count; };

// Transpile to AMD/RequireJS:
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

// Transpile to UMD/UmdJS:
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

// Transpile to System/SystemJS:
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

const ts = {
    "compilerOptions": {
        "module": "ES2020", // None, CommonJS, AMD, System, UMD, ES6, ES2015, ES2020, ESNext.
    }
};