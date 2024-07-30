// This is used to cause an error, because throwing an exception from the console does not invoke the handler.
function causeErrors() {
    if (Math.random() < 0.33) {
        undefinedfunction1();
        //throw new SyntaxError("Message 1 was not caught.");
    } else if (Math.random() < 0.6) {
        undefinedfunction2();
        //throw new DOMException("Message 2 was not caught.", "OperationError");
    } else {
        undefinedfunction3();
        //throw new DOMException("Message 3 was not caught.", "SyntaxError");
    }
}
