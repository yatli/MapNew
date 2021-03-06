namespace MapNew

open System.Collections.Generic

module Sorting =


    let inline private mergeSeq (cmp : IComparer<'Key>) (li : int) (ri : int) (len : int) (src : ('Key * 'Value)[]) (dst : ('Key * 'Value)[]) (length : int) =
        let le = ri
        let re = min length (ri + len)
        let mutable oi = li
        let mutable li = li
        let mutable ri = ri

        while li < le && ri < re do
            let lv = src.[li]
            let rv = src.[ri]
            let c = cmp.Compare(fst lv, fst rv)
            if c <= 0 then
                dst.[oi] <- lv
                oi <- oi + 1
                li <- li + 1
            else
                dst.[oi] <- rv
                oi <- oi + 1
                ri <- ri + 1

        while li < le do
            dst.[oi] <- src.[li]
            oi <- oi + 1
            li <- li + 1
                
        while ri < re do
            dst.[oi] <- src.[ri]
            oi <- oi + 1
            ri <- ri + 1

    let inline private mergeSeqHandleDuplicates (cmp : IComparer<'Key>) (li : int) (ri : int) (len : int) (src : ('Key * 'Value)[]) (dst : ('Key * 'Value)[]) (length : int) =
        let le = ri
        let re = min length (ri + len)
        let start = li
        let mutable oi = li
        let mutable li = li
        let mutable ri = ri
        let mutable lastValue = Unchecked.defaultof<'Key * 'Value>

        let inline append (v : ('Key * 'Value)) =
            if oi > start && cmp.Compare(fst v, fst lastValue) = 0 then
                dst.[oi-1] <- v
                lastValue <- v
            else
                dst.[oi] <- v
                lastValue <- v
                oi <- oi + 1

        while li < le && ri < re do
            let lv = src.[li]
            let rv = src.[ri]
            let c = cmp.Compare(fst lv, fst rv)
            if c <= 0 then
                append lv
                li <- li + 1
            else
                append rv
                ri <- ri + 1

        while li < le do
            append src.[li]
            li <- li + 1
                
        while ri < re do
            append src.[ri]
            ri <- ri + 1

        oi
        
    // assumes length > 2
    let mergeSortHandleDuplicates (mutateArray : bool) (cmp : IComparer<'Key>) (arr : ('Key * 'Value)[]) (length : int) =
        let mutable src = Array.zeroCreate length
        let mutable dst = 
            // mutateArray => allowed to mutate arr
            if mutateArray then arr
            else Array.zeroCreate length

        // copy to sorted pairs
        let mutable i0 = 0
        let mutable i1 = 1
        while i1 < length do
            let va = arr.[i0]
            let vb = arr.[i1]
            let c = cmp.Compare(fst va, fst vb)
            if c <= 0 then
                src.[i0] <- va
                src.[i1] <- vb
            else
                src.[i0] <- vb
                src.[i1] <- va
                    
            i0 <- i0 + 2
            i1 <- i1 + 2

        if i0 < length then
            src.[i0] <- arr.[i0]
            i0 <- i0 + 1

        let arr = ()

        // merge sorted parts of length `sortedLength`
        let mutable sortedLength = 2
        let mutable sortedLengthDbl = 4
        while sortedLengthDbl < length do
            let mutable li = 0
            let mutable ri = sortedLength

            // merge case
            while ri < length do
                mergeSeq cmp li ri sortedLength src dst length
                li <- ri + sortedLength
                ri <- li + sortedLength

            // right got empty
            while li < length do
                dst.[li] <- src.[li]
                li <- li + 1
                    
            // sortedLength * 2
            sortedLength <- sortedLengthDbl
            sortedLengthDbl <- sortedLengthDbl <<< 1
            // swap src and dst
            let t = dst
            dst <- src
            src <- t

        // final merge-dedup run
        let cnt = mergeSeqHandleDuplicates cmp 0 sortedLength sortedLength src dst length
        struct(dst, cnt)

    let inline private mergeSeqV (cmp : IComparer<'Key>) (li : int) (ri : int) (len : int) (src : struct('Key * 'Value)[]) (dst : struct('Key * 'Value)[]) (length : int) =
        let le = ri
        let re = min length (ri + len)
        let mutable oi = li
        let mutable li = li
        let mutable ri = ri

        while li < le && ri < re do
            let struct(lk, lv) = src.[li]
            let struct(rk, rv) = src.[ri]
            let c = cmp.Compare(lk, rk)
            if c <= 0 then
                dst.[oi] <- struct(lk, lv)
                oi <- oi + 1
                li <- li + 1
            else
                dst.[oi] <- struct(rk, rv)
                oi <- oi + 1
                ri <- ri + 1

        while li < le do
            dst.[oi] <- src.[li]
            oi <- oi + 1
            li <- li + 1
                
        while ri < re do
            dst.[oi] <- src.[ri]
            oi <- oi + 1
            ri <- ri + 1

    let inline private mergeSeqHandleDuplicatesV (cmp : IComparer<'Key>) (li : int) (ri : int) (len : int) (src : struct('Key * 'Value)[]) (dst : struct('Key * 'Value)[]) (length : int) =
        let le = ri
        let re = min length (ri + len)
        let start = li
        let mutable oi = li
        let mutable li = li
        let mutable ri = ri
        let mutable lastKey = Unchecked.defaultof<'Key>

        let inline append k v =
            if oi > start && cmp.Compare(k, lastKey) = 0 then
                dst.[oi-1] <- struct(k,v)
                lastKey <- k
            else
                dst.[oi] <- struct(k,v)
                lastKey <- k
                oi <- oi + 1

        while li < le && ri < re do
            let struct(lk, lv) = src.[li]
            let struct(rk, rv) = src.[ri]
            let c = cmp.Compare(lk, rk)
            if c <= 0 then
                append lk lv
                li <- li + 1
            else
                append rk rv
                ri <- ri + 1

        while li < le do
            let struct(k,v) = src.[li]
            append k v
            li <- li + 1
                
        while ri < re do
            let struct(k,v) = src.[ri]
            append k v
            ri <- ri + 1

        oi
        
    // assumes length > 2
    let mergeSortHandleDuplicatesV (mutateArray : bool) (cmp : IComparer<'Key>) (arr : struct('Key * 'Value)[]) (length : int) =
        let mutable src = Array.zeroCreate length
        let mutable dst = 
            // mutateArray => allowed to mutate arr
            if mutateArray then arr
            else Array.zeroCreate length

        // copy to sorted pairs
        let mutable i0 = 0
        let mutable i1 = 1
        while i1 < length do
            let struct(ka,va) = arr.[i0] 
            let struct(kb,vb) = arr.[i1]

            let c = cmp.Compare(ka, kb)
            if c <= 0 then
                src.[i0] <- struct(ka, va)
                src.[i1] <- struct(kb, vb)
            else
                src.[i0] <- struct(kb, vb)
                src.[i1] <- struct(ka, va)
                    
            i0 <- i0 + 2
            i1 <- i1 + 2

        if i0 < length then
            src.[i0] <- arr.[i0]
            i0 <- i0 + 1

        let arr = ()

        // merge sorted parts of length `sortedLength`
        let mutable sortedLength = 2
        let mutable sortedLengthDbl = 4
        while sortedLengthDbl < length do
            let mutable li = 0
            let mutable ri = sortedLength

            // merge case
            while ri < length do
                mergeSeqV cmp li ri sortedLength src dst length
                li <- ri + sortedLength
                ri <- li + sortedLength

            // right got empty
            while li < length do
                dst.[li] <- src.[li]
                li <- li + 1
                    
            // sortedLength * 2
            sortedLength <- sortedLengthDbl
            sortedLengthDbl <- sortedLengthDbl <<< 1
            // swap src and dst
            let t = dst
            dst <- src
            src <- t


        // final merge-dedup run
        let cnt = mergeSeqHandleDuplicatesV cmp 0 sortedLength sortedLength src dst length
        struct(dst, cnt)