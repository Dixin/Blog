const path = require('path');

module.exports = {
    entry: './src/index.js',
    mode: "none", // Do not optimize or minimize the code for readability.
    output: {
        filename: 'main.js',
        path: path.resolve(__dirname, 'dist'),
    },
};

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
        const dependency1 = require(2);
        const api2 = () => dependency1.api1();
        exports.api2 = api2;
    }
]);