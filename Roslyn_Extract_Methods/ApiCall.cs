namespace Roslyn_Extract_Methods {
    class ApiCall {
        public string Call { get; }

        private ApiCall(string call) {
            Call = call;
        }

        public override string ToString() {
            return Call;
        }

        public static ApiCall OfMethodInvocation(string className, string methodName) {
            if (className == "") return new ApiCall(methodName);
            return new ApiCall(className + "." + methodName);
        }

        public static ApiCall OfConstructor(string className) {
            return new ApiCall(className + ".new");
        }
    }
}