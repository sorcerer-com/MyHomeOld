package com.my.home.Utils;

import com.my.home.TcpConnection.Client;

import java.util.concurrent.ConcurrentLinkedQueue;

/**
 * Created by Hristo on 14-02-28.
 */
public class WorkerThread implements Runnable {

    private static final int numThreads = 2;
	private static Thread[] threads;
	private static boolean alive = true;
	private static ConcurrentLinkedQueue <Runnable> queue;
	
	public static boolean isAlive() { return WorkerThread.alive; }
	
	
	public static void start() {
		WorkerThread.alive = true;
		WorkerThread.queue = new ConcurrentLinkedQueue<Runnable>();
        WorkerThread.threads = new Thread[WorkerThread.numThreads];
        for (int i = 0; i < WorkerThread.numThreads; i++) {
            WorkerThread.threads[i] = new Thread(new WorkerThread());
            WorkerThread.threads[i].setName("Worker Thread " + i);
            WorkerThread.threads[i].start();
        }
	}
	
	public static void stop() {
		WorkerThread.alive = false;
		for (int i = 0; i < WorkerThread.numThreads; i++)
			Utils.Thread.join(WorkerThread.threads[i]);
		WorkerThread.threads = null;
		WorkerThread.queue.clear();
		WorkerThread.queue = null;
	}
	
	public static void execute(Runnable run) {
		if (WorkerThread.threads == null)
			WorkerThread.start();

        WorkerThread.queue.offer(run);
	}
	
	
	private WorkerThread() {
	}
	
	@Override
	public void run() {
		while (WorkerThread.alive) {
			Runnable run = WorkerThread.queue.poll();
			
			if (run != null) {
				try {
					run.run();
				} catch (Exception e) {
                    Client.log("Exception: " + e.getMessage());
                }
			}
			else
				Utils.Thread.sleep(10);
		}
	}
	
}
