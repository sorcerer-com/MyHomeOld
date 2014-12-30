package com.my.home.TcpConnection;

import android.os.Handler;
import android.os.Looper;

import com.my.home.Utils.Utils;
import com.my.home.Utils.WorkerThread;

import java.io.ByteArrayOutputStream;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.InetSocketAddress;
import java.net.Socket;
import java.net.SocketTimeoutException;
import java.util.ArrayList;

/**
 * Created by Hristo on 13-12-28.
 */
public class Client {

    public interface IsConnectedListener {
        public void onIsConnectedChanged(boolean isConnected);
    }

    public interface ReceivedListener {
        public void onCommandReceived(Client client, Command command);
    }

    public static ArrayList<String> Log = new ArrayList<String>();

    public static void log(String str) {
        if (Log.size() > 1000)
            Log.remove(0);
        Log.add(str);
    }

	
    private String address;
    private int port;

	private boolean threadAlive;
	private Thread thread;
    private Socket socket;

	
    public boolean isConnected() {
        return this.socket != null && this.socket.isConnected();
    }

    public String getAddress() { return this.address; }
    public void setAddress(String address) { this.address = address; }

    public int getPort() { return this.port; }
    public void setPort(int port) { this.port = port; }


    public Client(String address, int port) {
        this.address = address;
        this.port = port;

		this.threadAlive = true;
        this.thread = new Thread(new Receiver());
        this.thread.setName("Client Receiver Thread");
        this.thread.start();

        this.socket = null;
    }

    @Override
    public void finalize() {
		this.threadAlive = false;
		Utils.Thread.join(this.thread);
		
        this.isConnectedListeners.clear();
        this.commandReceivedListeners.clear();
        this.disconnect();

        try {
            super.finalize();
        } catch (Throwable e) {
            Client.log("Exception: " + e.getMessage());
        }
    }

    public void connect() {
        if (this.socket != null && this.socket.isConnected())
            return;

		this.socket = null;
        WorkerThread.execute( new Runnable() {
            @Override
            public void run() {
                InetSocketAddress serverAddress;
                try {
                    serverAddress = new InetSocketAddress(Client.this.address, Client.this.port);
                    Client.this.socket = new Socket();
                    Client.this.socket.connect(serverAddress, 1000);
                    Client.this.socket.setSoTimeout(1000);
                    Client.log("Socket connected to " + Client.this.socket.getLocalSocketAddress().toString());
                } catch (Exception e) {
                    Client.log("Exception: " + e.getMessage());
                }
                Client.this.onIsConnectedChanged();
            }
        });
        Client.this.onIsConnectedChanged();
    }

    public void disconnect() {
        if (this.socket == null)
            return;

        WorkerThread.execute( new Runnable() {
            @Override
            public void run() {
                try {
                    Client.this.socket.shutdownInput();
                    Client.this.socket.shutdownOutput();
                    Client.this.socket.close();
                    Client.this.socket = null;
                    Client.log("Disconnect");
                } catch (Exception e) {
                    Client.log("Exception: " + e.getMessage());
                }
                Client.this.onIsConnectedChanged();
            }
        });
    }

    public boolean send(final Command command) {
        if (this.socket == null || !this.socket.isConnected()) {
            this.connect();
            if (this.socket == null || !this.socket.isConnected())
                return false;
        }

        WorkerThread.execute( new Runnable() {
            @Override
            public void run() {
                try {
                    OutputStream out = Client.this.socket.getOutputStream();

                    byte[] bytes = command.serialize();
                    out.write(bytes);
                    Client.log("Send command: " + command.toString());
                } catch (Exception e) {
                    Client.log("Exception: " + e.getMessage());
                }
            }
        });
        return true;
    }

    private class Receiver implements Runnable {
        @Override
        public void run() {
			ByteArrayOutputStream buffer = new ByteArrayOutputStream();
            while (Client.this.threadAlive) {
                try {
                    Socket socket = Client.this.socket;
                    if (socket == null || !socket.isConnected()) {
                        Thread.sleep(100);
                        continue;
                    }
                    InputStream in = socket.getInputStream();
                    if (in.available() < Command.MinBytes) {
                        int oneByte = socket.getInputStream().read();
                        if (oneByte == -1)
                            Client.this.disconnect();
                        else
                            buffer.write(oneByte);

                        if (buffer.size() < Command.MinBytes) {
                            Thread.sleep(100);
                            continue;
                        }
                    }

                    while (in.available() > 0) {
                        byte[] bytes = new byte[in.available()];
                        int bytesRec = in.read(bytes);
						buffer.write(bytes, 0, bytesRec);
                        Thread.sleep(100);
                    }
					byte[] data = buffer.toByteArray();
					buffer.reset();
                    while (data.length > 0) {
                        Command cmd = new Command();
                        data = cmd.deSerialize(data);
                        Client.log("Received command: " + cmd.toString());
                        Client.this.onCommandReceived(cmd);
                    }
                } catch (Exception e) {
                    if (!SocketTimeoutException.class.isInstance(e)) {
                        Client.log("Exception: " + e.getMessage());
                        Client.this.disconnect();
                    }
                }
            }
            try {
                buffer.close();
            } catch (Exception e) {
                Client.log("Exception: " + e.getMessage());
            }
        }
    }


    private ArrayList<IsConnectedListener> isConnectedListeners = new ArrayList<IsConnectedListener>();
    public void registerIsConnectedListener(IsConnectedListener listener) {
		if (!this.isConnectedListeners.contains(listener))
			this.isConnectedListeners.add(listener);
    }

    public void unregisterIsConnectedListener(IsConnectedListener listener) {
        if (this.isConnectedListeners.contains(listener))
            this.isConnectedListeners.remove(listener);
    }

    private void onIsConnectedChanged() {
        final boolean isConnected = this.isConnected();
		
		Handler handler = new Handler(Looper.getMainLooper());
        handler.post( new Runnable() {
            @Override
            public void run() {
				for (IsConnectedListener listener : Client.this.isConnectedListeners)
					listener.onIsConnectedChanged(isConnected);
            }
        });
    }

    private ArrayList<ReceivedListener> commandReceivedListeners = new ArrayList<ReceivedListener>();
    public void registerCommandReceivedListener(ReceivedListener listener) {
		if (!this.commandReceivedListeners.contains(listener))
			this.commandReceivedListeners.add(listener);
    }

    public void unregisterCommandReceivedListener(ReceivedListener listener) {
        if (this.commandReceivedListeners.contains(listener))
            this.commandReceivedListeners.remove(listener);
    }

    private void onCommandReceived(Command cmd) {
        for (ReceivedListener listener : this.commandReceivedListeners)
            listener.onCommandReceived(this, cmd);
    }

}
