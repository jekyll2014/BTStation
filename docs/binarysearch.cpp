byte BinarySearch(byte A[], byte key)
{
int left=0, right=N, mid;
while (left<=right)
{
mid=left+(right-left)/2;
if (key<A[mid]) right=mid-1;
else if (key>A[mid]) left=mid+1;
else return mid;
}
return -1;
}
