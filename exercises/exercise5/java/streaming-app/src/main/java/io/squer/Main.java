package io.squer;

import io.squer.services.StreamingAppWorker;

public class Main {

    public static void main(String[] args) {

        StreamingAppWorker worker = new StreamingAppWorker();

        worker.run();
    }



}
